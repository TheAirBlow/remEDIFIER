#include "BluetoothAdapter.h"
#include <ApplicationLoop.h>
#include <qthread.h>

BluetoothAdapter* CreateBluetoothAdapter() {
    auto obj = new BluetoothAdapter();
    QThread* workerThread = new QThread();
    workerThread->start();
    obj->moveToThread(workerThread);
    QMetaObject::invokeMethod(obj, "enumerate", Qt::BlockingQueuedConnection);
    return obj;
}

void SetAdapterEnabledCallback(BluetoothAdapter* manager, AdapterCallback callback) {
    manager->enabledCallback = callback;
}

void SetAdapterDisabledCallback(BluetoothAdapter* manager, AdapterCallback callback) {
    manager->disabledCallback = callback;
}

const char* GetAdapterAddress(BluetoothAdapter* manager) {
    return strdup(manager->getAddress().toString().toLocal8Bit().data());
}

ConnectedDevices* GetConnectedDevices(BluetoothAdapter* manager) {
    ConnectedDevices* devices;
    QMetaObject::invokeMethod(manager, "getConnectedDevices",
        Qt::BlockingQueuedConnection, Q_RETURN_ARG(ConnectedDevices*, devices));
    return devices;
}

bool IsBluetoothAvailable(BluetoothAdapter* manager) {
    return manager->isBluetoothAvailable();
}
