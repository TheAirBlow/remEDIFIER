#include "BluetoothClassic.h"

BluetoothClassic* CreateBluetoothClassic() {
    auto obj = new BluetoothClassic();
    obj->moveToThread(GetMainThread());
    obj->setParent(GetApplication());
    return obj;
}

void ClassicConnect(BluetoothClassic* manager, const char* address) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginConnect(address);
    });
}

void ClassicDisconnect(BluetoothClassic* manager) {
    QMetaObject::invokeMethod(manager, [=]() {
        manager->beginDisconnect();
    });
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
    });
}
