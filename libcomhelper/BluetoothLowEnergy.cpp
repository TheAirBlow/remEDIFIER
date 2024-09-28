#include "BluetoothLowEnergy.h"

BluetoothLowEnergy* CreateBluetoothLowEnergy() {
    auto obj = new BluetoothLowEnergy();
    obj->moveToThread(GetMainThread());
    obj->setParent(GetApplication());
    return obj;
}

void LowEnergyConnect(BluetoothLowEnergy* manager, const char* macAddress, const char* serviceUuid, const char* writeUuid, const char* readUuid) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginConnect(macAddress, serviceUuid, writeUuid, readUuid);
    });
}

void LowEnergyDisconnect(BluetoothLowEnergy* manager) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginDisconnect();
    });
}

void SetLowEnergyDisconnectedCallback(BluetoothLowEnergy* manager, GenericCallback callback) {
    manager->disconnectedCallback = callback;
}

void SetLowEnergyConnectedCallback(BluetoothLowEnergy* manager, GenericCallback callback) {
    manager->connectedCallback = callback;
}

void SetLowEnergyErrorCallback(BluetoothLowEnergy* manager, ErrorCallback callback) {
    manager->errorCallback = callback;
}

void SetLowEnergyDataCallback(BluetoothLowEnergy* manager, DataCallback callback) {
    manager->dataCallback = callback;
}

void LowEnergyWrite(BluetoothLowEnergy* manager, const char* data, uint32_t length) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->write(data, length);
    });
}
