#include "BluetoothDiscovery.h"

#include <QBluetoothDeviceDiscoveryAgent>
#include <QBluetoothLocalDevice>
#include <QBluetoothDeviceInfo>
#include <ApplicationLoop.h>
#include <QThread>
#include <QTimer>

BluetoothDiscovery* CreateDiscovery() {
    auto obj = new BluetoothDiscovery();
    obj->moveToThread(GetMainThread());
    obj->setParent(GetApplication());
    return obj;
}

void SetDeviceDiscoveredCallback(BluetoothDiscovery* wrapper, DeviceDiscoveredCallback callback) {
    wrapper->discoveredCallback = callback;
}

void SetDiscoveryFinishedCallback(BluetoothDiscovery* wrapper, DiscoveryFinishedCallback callback) {
    wrapper->finishedCallback = callback;
}

void StartDiscovery(BluetoothDiscovery* wrapper) {
    QMetaObject::invokeMethod(wrapper, [=]() {
        wrapper->startDiscovery();
    });
}

void StopDiscovery(BluetoothDiscovery* wrapper) {
    QMetaObject::invokeMethod(wrapper, [=]() {
        wrapper->stopDiscovery();
    });
}
