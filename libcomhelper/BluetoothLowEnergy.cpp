#include "BluetoothLowEnergy.h"

#include <qthread.h>

BluetoothLowEnergy* CreateBluetoothLowEnergy() {
    auto obj = new BluetoothLowEnergy();
    QThread* workerThread = new QThread();
    workerThread->start();
    obj->moveToThread(workerThread);
    return obj;
}

void LowEnergyConnect(BluetoothLowEnergy* manager, const char* localAddress, const char* macAddress, const char* serviceUuid, const char* writeUuid, const char* readUuid) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginConnect(localAddress, macAddress, serviceUuid, writeUuid, readUuid);
    }, Qt::BlockingQueuedConnection);
}

void LowEnergyDisconnect(BluetoothLowEnergy* manager) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginDisconnect();
    }, Qt::BlockingQueuedConnection);
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
    }, Qt::BlockingQueuedConnection);
}
