#ifndef BLUETOOTHDISCOVERY_H
#define BLUETOOTHDISCOVERY_H

#include <QBluetoothDeviceDiscoveryAgent>
#include <QBluetoothUuid>
#include <QObject>
#include <QDebug>
#include <QHash>

struct DeviceInfo {
    const char* DeviceName;
    const char* MacAddress;
    bool IsLowEnergyDevice;
    const char** ServiceUuids;
    uint32_t ServiceUuidsLength;
    const char* ManufacturerData;
    uint32_t ManufacturerDataLength;
    uint16_t ManufacturerId;
    uint32_t MajorDeviceClass;
    uint32_t MinorDeviceClass;
};

typedef void (*DeviceDiscoveredCallback)(DeviceInfo info);
typedef void (*DiscoveryFinishedCallback)();

class BluetoothDiscovery : public QObject {
    Q_OBJECT

public:
    BluetoothDiscovery() {
        discoveryAgent = new QBluetoothDeviceDiscoveryAgent(this);
        connect(discoveryAgent, &QBluetoothDeviceDiscoveryAgent::deviceDiscovered, this, &BluetoothDiscovery::onDeviceDiscovered);
        connect(discoveryAgent, &QBluetoothDeviceDiscoveryAgent::finished, this, &BluetoothDiscovery::onFinished);
    }

public slots:
    void startDiscovery() {
        discoveryAgent->start(QBluetoothDeviceDiscoveryAgent::ClassicMethod | QBluetoothDeviceDiscoveryAgent::LowEnergyMethod);
    }

    void stopDiscovery() {
        discoveryAgent->stop();
    }

private slots:
    void onDeviceDiscovered(const QBluetoothDeviceInfo& deviceInfo) {
        if (discoveredCallback) {
            QByteArray nameBuf = deviceInfo.name().toLocal8Bit();
#ifdef Q_OS_MAC
            QByteArray addressBuf = deviceInfo.deviceUuid().toString().toLocal8Bit();
#else
            QByteArray addressBuf = deviceInfo.address().toString().toLocal8Bit();
#endif
            DeviceInfo info{0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            info.DeviceName = strdup(nameBuf.data());
            info.MacAddress = strdup(addressBuf.data());
            info.IsLowEnergyDevice = deviceInfo.coreConfigurations() & QBluetoothDeviceInfo::LowEnergyCoreConfiguration;
            if (info.IsLowEnergyDevice) {
                QHash<unsigned short, QByteArray> data = deviceInfo.manufacturerData();
                if (data.empty()) return;
                QList<QBluetoothUuid> uuids = deviceInfo.serviceUuids();
                info.ServiceUuids = new const char*[uuids.length()];
                for (int i = 0; i < uuids.length(); i++) {
                    const char* data = strdup(uuids.at(i).toString().toLocal8Bit().data());
                    info.ServiceUuids[i] = data;
                }
                info.ServiceUuidsLength = uuids.length();
                info.ManufacturerId = data.keys().constFirst();
                QByteArray buffer = data.values().constFirst();
                info.ManufacturerData = strdup(buffer.data());
                info.ManufacturerDataLength = buffer.length();
            } else {
                info.MajorDeviceClass = deviceInfo.majorDeviceClass();
                info.MinorDeviceClass = deviceInfo.minorDeviceClass();
            }

            discoveredCallback(info);
        }
    }

    void onFinished() {
        if (finishedCallback) {
            finishedCallback();
        }
    }

private:
    QBluetoothDeviceDiscoveryAgent* discoveryAgent;

public:
    DeviceDiscoveredCallback discoveredCallback;
    DiscoveryFinishedCallback finishedCallback;
};

extern "C" {
BluetoothDiscovery* CreateDiscovery();
void SetDeviceDiscoveredCallback(BluetoothDiscovery* wrapper, DeviceDiscoveredCallback callback);
void SetDiscoveryFinishedCallback(BluetoothDiscovery* wrapper, DiscoveryFinishedCallback callback);
void StartDiscovery(BluetoothDiscovery* wrapper);
void StopDiscovery(BluetoothDiscovery* wrapper);
}

#endif // BLUETOOTHDISCOVERY_H
