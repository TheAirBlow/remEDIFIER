#ifndef BLUETOOTHLOWENERGY_H
#define BLUETOOTHLOWENERGY_H

#include <QLowEnergyController>
#include <BluetoothAdapter.h>
#include <ApplicationLoop.h>
#include <QObject>
#include <QDebug>

struct DeviceConnectInfo {
    const char* MacAddress;
    const char* ServiceUuid;
    const char* WriteUuid;
    const char* ReadUuid;
};

typedef void (*GenericCallback)();
typedef void (*DataCallback)(const char* data, uint32_t length);
typedef void (*ErrorCallback)(const char* message, int code);

class BluetoothLowEnergy : public QObject {
    Q_OBJECT

public:
    BluetoothLowEnergy() {
        adapter = new BluetoothAdapter();
        adapter->setParent(this);
    }

    void beginConnect(const char* macAddress, const char* serviceUuid, const char* writeUuid, const char* readUuid) {
        if (connected) return;
        auto localAddress = adapter->getAddress();
        if (localAddress.isNull()) {
            if (errorCallback) errorCallback("No bluetooth adapter available", -2);
            return;
        }

        this->device = new DeviceConnectInfo {
            macAddress, serviceUuid, writeUuid, readUuid
        };
        auto address = QString::fromUtf8(this->device->MacAddress);
        controller = QLowEnergyController::createCentral(QBluetoothAddress(address), localAddress, this);
        connect(controller, &QLowEnergyController::connected, controller, &QLowEnergyController::discoverServices);
        connect(controller, QOverload<QLowEnergyController::Error>::of(&QLowEnergyController::error), this, &BluetoothLowEnergy::onErrorOccurred);
        connect(controller, &QLowEnergyController::serviceDiscovered, this, &BluetoothLowEnergy::onServiceDiscovered);
        controller->connectToDevice();
    }

    void beginDisconnect() {
        if (!connected) return;
        connected = false;
        if (service) {
            if (readCharacteristic.isValid()) {
                auto desc = readCharacteristic.descriptor(QBluetoothUuid::ClientCharacteristicConfiguration);
                service->writeDescriptor(desc, QByteArray::fromHex("0000")); // DISABLE_NOTIFICATION_VALUE
            }

            service->deleteLater();
            service = nullptr;
        }

        if (controller) {
            controller->disconnectFromDevice();
            controller->deleteLater();
            controller = nullptr;
        }

        if (disconnectedCallback) disconnectedCallback();
    }

    void write(const char* data, uint32_t length) {
        auto buf = QByteArray::fromRawData(data, length);
        service->writeCharacteristic(writeCharacteristic, buf);
    }

private slots:
    void onErrorOccurred() {
        if (errorCallback) errorCallback(controller->errorString().toLocal8Bit().data(), controller->error());
        beginDisconnect();
    }

    void onServiceDiscovered(const QBluetoothUuid& uuid) {
        auto expectedUuid = QBluetoothUuid(QString::fromUtf8(device->ServiceUuid));
        if (uuid != expectedUuid) return;
        service = controller->createServiceObject(uuid);
        connect(service, &QLowEnergyService::stateChanged, this, &BluetoothLowEnergy::onDetailsDiscovered);
        service->discoverDetails();
    }

    void onDetailsDiscovered(QLowEnergyService::ServiceState state) {
        if (state == QLowEnergyService::ServiceDiscovered) {
            auto writeUuid = QBluetoothUuid(QString::fromUtf8(device->WriteUuid));
            auto readUuid = QBluetoothUuid(QString::fromUtf8(device->ReadUuid));
            const QList<QLowEnergyCharacteristic> chars = service->characteristics();

            for (auto ch : chars) {
                if (ch.properties().testFlag(QLowEnergyCharacteristic::Notify) && ch.uuid() == readUuid)
                    readCharacteristic = ch;
                if (ch.properties().testFlag(QLowEnergyCharacteristic::Write) && ch.uuid() == writeUuid)
                    writeCharacteristic = ch;
            }

            if (readCharacteristic.isValid() && writeCharacteristic.isValid()) {
                connect(service, &QLowEnergyService::stateChanged, this, &BluetoothLowEnergy::onServiceStateChanged);
                connect(service, QOverload<QLowEnergyService::ServiceError>::of(&QLowEnergyService::error), this, &BluetoothLowEnergy::onErrorOccurred);
                connect(service, &QLowEnergyService::characteristicChanged, this, &BluetoothLowEnergy::onDataReceived);
                QLowEnergyDescriptor desc = readCharacteristic.descriptor(QBluetoothUuid::ClientCharacteristicConfiguration);
                service->writeDescriptor(desc, QByteArray::fromHex("0100")); // ENABLE_NOTIFICATION_VALUE
                if (connectedCallback) connectedCallback();
                connected = true;
                return;
            }

            if (errorCallback) errorCallback("Failed to find valid Rx/Tx characteristic", -3);
            beginDisconnect();
        }

        if (state == QLowEnergyService::InvalidService) {
            if (errorCallback) errorCallback("Device no longer exists", -5);
            beginDisconnect();
        }
    }

    void onDataReceived(const QLowEnergyCharacteristic &characteristic, const QByteArray &value) {
        Q_UNUSED(characteristic);
        if (!dataCallback) return;
        dataCallback(strdup(value.data()), value.length());
    }

    void onServiceStateChanged(QLowEnergyService::ServiceState state) {
        if(state == QLowEnergyService::InvalidService) beginDisconnect();
    }

private:
    QLowEnergyCharacteristic writeCharacteristic;
    QLowEnergyCharacteristic readCharacteristic;
    QLowEnergyController* controller;
    QLowEnergyService* service;
    DeviceConnectInfo* device;
    BluetoothAdapter* adapter;
    bool connected;

public:
    GenericCallback disconnectedCallback;
    GenericCallback connectedCallback;
    ErrorCallback errorCallback;
    DataCallback dataCallback;
};

extern "C" {
BluetoothLowEnergy* CreateBluetoothLowEnergy();
void LowEnergyConnect(BluetoothLowEnergy* manager, const char* macAddress, const char* serviceUuid, const char* writeUuid, const char* readUuid);
void LowEnergyDisconnect(BluetoothLowEnergy* manager);
void SetLowEnergyDisconnectedCallback(BluetoothLowEnergy* manager, GenericCallback callback);
void SetLowEnergyConnectedCallback(BluetoothLowEnergy* manager, GenericCallback callback);
void SetLowEnergyErrorCallback(BluetoothLowEnergy* manager, ErrorCallback callback);
void SetLowEnergyDataCallback(BluetoothLowEnergy* manager, DataCallback callback);
void LowEnergyWrite(BluetoothLowEnergy* manager, const char* data, uint32_t length);
}

#endif // BLUETOOTHLOWENERGY_H
