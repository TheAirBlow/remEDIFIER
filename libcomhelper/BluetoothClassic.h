#ifndef BLUETOOTHCLASSIC_H
#define BLUETOOTHCLASSIC_H

#include <QBluetoothSocket>
#include <BluetoothLowEnergy.h>
#include <QObject>

class BluetoothClassic : public QObject {
    Q_OBJECT

public slots:
    void beginConnect(const char* address){
        socket = new QBluetoothSocket(QBluetoothServiceInfo::RfcommProtocol, this);
        connect(socket, &QBluetoothSocket::stateChanged, this, &BluetoothClassic::onStateChanged);
        connect(socket, &QBluetoothSocket::readyRead, this, &BluetoothClassic::onDataReceived);
        connect(socket, QOverload<QBluetoothSocket::SocketError>::of(&QBluetoothSocket::error), this, &BluetoothClassic::onErrorOccurred);
        socket->connectToService(QBluetoothAddress(QString::fromUtf8(address)), uuid);
    }

    void beginDisconnect() {
        resetCallbacks();
        socket->disconnectFromService();
        socket->close();
    }

    void write(const char* data, uint32_t length) {
        auto buf = QByteArray::fromRawData(data, length);
        socket->write(buf);
    }

private slots:
    void onStateChanged() {
        QBluetoothSocket::SocketState state = socket->state();
        if (state == QBluetoothSocket::ConnectedState){
            if (connectedCallback) connectedCallback();
        }

        if (state == QBluetoothSocket::UnconnectedState){
            if (disconnectedCallback) disconnectedCallback();
            beginDisconnect();
        }
    }

    void onErrorOccurred() {
        if (errorCallback) errorCallback(strdup(socket->errorString().toLocal8Bit().data()), socket->error());
        beginDisconnect();
    }

    void onDataReceived() {
        QByteArray value = socket->readAll();
        if (!dataCallback) return;
        dataCallback(value.data(), value.length());
    }

    void resetCallbacks() {
        disconnectedCallback = nullptr;
        connectedCallback = nullptr;
        errorCallback = nullptr;
        dataCallback = nullptr;
    }

private:
    const QBluetoothUuid uuid = QBluetoothUuid(QStringLiteral("EDF00000-EDFE-DFED-FEDF-EDFEDFEDFEDF"));
    QBluetoothSocket* socket;

public:
    GenericCallback disconnectedCallback;
    GenericCallback connectedCallback;
    ErrorCallback errorCallback;
    DataCallback dataCallback;
};

extern "C" {
BluetoothClassic* CreateBluetoothClassic();
void ClassicConnect(BluetoothClassic* manager, const char* address);
void ClassicDisconnect(BluetoothClassic* manager);
void SetClassicDisconnectedCallback(BluetoothClassic* manager, GenericCallback callback);
void SetClassicConnectedCallback(BluetoothClassic* manager, GenericCallback callback);
void SetClassicErrorCallback(BluetoothClassic* manager, ErrorCallback callback);
void SetClassicDataCallback(BluetoothClassic* manager, DataCallback callback);
void ClassicWrite(BluetoothClassic* manager, const char* data, uint32_t length);
}

#endif // BLUETOOTHCLASSIC_H
