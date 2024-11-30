#include "BluetoothClassic.h"

#include <qthread.h>

BluetoothClassic* CreateBluetoothClassic() {
    auto obj = new BluetoothClassic();
    QThread* workerThread = new QThread();
    workerThread->start();
    obj->moveToThread(workerThread);
    return obj;
}

void ClassicConnect(BluetoothClassic* manager, const char* address) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginConnect(address);
    }, Qt::BlockingQueuedConnection);
}

void ClassicDisconnect(BluetoothClassic* manager) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginDisconnect();
    }, Qt::BlockingQueuedConnection);
}

void SetClassicDisconnectedCallback(BluetoothClassic* manager, GenericCallback callback) {
    manager->disconnectedCallback = callback;
}

void SetClassicConnectedCallback(BluetoothClassic* manager, GenericCallback callback) {
    manager->connectedCallback = callback;
}

void SetClassicErrorCallback(BluetoothClassic* manager, ErrorCallback callback) {
    manager->errorCallback = callback;
}

void SetClassicDataCallback(BluetoothClassic* manager, DataCallback callback) {
    manager->dataCallback = callback;
}

void ClassicWrite(BluetoothClassic* manager, const char* data, uint32_t length) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->write(data, length);
    }, Qt::BlockingQueuedConnection);
}
