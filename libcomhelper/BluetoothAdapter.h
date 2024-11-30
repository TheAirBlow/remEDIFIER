#ifndef BLUETOOTHADAPTER_H
#define BLUETOOTHADAPTER_H

#include <QBluetoothLocalDevice>
#include <QObject>
#include <QDebug>

struct ConnectedDevices {
    const char** Addresses;
    uint32_t Length;
};

Q_DECLARE_METATYPE(ConnectedDevices*)

typedef void (*AdapterCallback)(const char* address);

class BluetoothAdapter : public QObject {
    Q_OBJECT

private slots:
    void hostModeStateChanged(QBluetoothLocalDevice::HostMode state) {
        auto device = (QBluetoothLocalDevice)sender();
        if (!adapter.isNull() && QString::compare(device.address().toString(), adapter.toString()) != 0) return;
        if (state == QBluetoothLocalDevice::HostPoweredOff) {
            adapter = QBluetoothAddress();
            if (disabledCallback) disabledCallback(strdup(device.address().toString().toLocal8Bit().data()));
            enumerate(false);
            return;
        }

        adapter = device.address();
        if (enabledCallback) enabledCallback(strdup(device.address().toString().toLocal8Bit().data()));
    }

public slots:
    void enumerate(bool shouldConnect = true) {
        auto devices = QBluetoothLocalDevice::allDevices();
        for (auto it = devices.cbegin(); it != devices.cend(); ++it) {
            QBluetoothLocalDevice* dev = new QBluetoothLocalDevice(it->address(), this); // memory leak my balls
            if (adapter.isNull() && dev->isValid() && dev->hostMode() != QBluetoothLocalDevice::HostPoweredOff) adapter = it->address();
            if (shouldConnect) connect(dev, &QBluetoothLocalDevice::hostModeStateChanged, this, &BluetoothAdapter::hostModeStateChanged, Qt::UniqueConnection);
        }
    }

    ConnectedDevices* getConnectedDevices() {
        if (adapter.isNull()) return nullptr;
        ConnectedDevices* info = new ConnectedDevices { 0, 0 };
        QBluetoothLocalDevice dev(adapter);
        auto connected = dev.connectedDevices();
        info->Length = connected.length();
        info->Addresses = new const char*[info->Length];
        for (uint32_t i = 0; i < info->Length; i++) {
            const char* data = strdup(connected.at(i).toString().toLocal8Bit().data());
            info->Addresses[i] = data;
        }

        return info;
    }

    QBluetoothAddress getAddress() {
        return adapter;
    }

    bool isBluetoothAvailable() {
        return !adapter.isNull();
    }

private:
    QBluetoothAddress adapter;

public:
    AdapterCallback enabledCallback;
    AdapterCallback disabledCallback;
};

extern "C" {
BluetoothAdapter* CreateBluetoothAdapter();
void SetAdapterEnabledCallback(BluetoothAdapter* manager, AdapterCallback callback);
void SetAdapterDisabledCallback(BluetoothAdapter* manager, AdapterCallback callback);
const char* GetAdapterAddress(BluetoothAdapter* manager);
ConnectedDevices* GetConnectedDevices(BluetoothAdapter* manager);
bool IsBluetoothAvailable(BluetoothAdapter* manager);
}

#endif // BLUETOOTHADAPTER_H
