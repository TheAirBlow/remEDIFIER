#include "ApplicationLoop.h"

#include <QBluetoothLocalDevice>
#include <BluetoothAdapter.h>
#include <QThread>

QCoreApplication* app;
QThread* thread;

QThread* GetMainThread() {
    return thread;
}

QCoreApplication* GetApplication() {
    return app;
}

void ExitApplication(int code) {
    app->exit(code);
}

int RunApplication() {
    char* argv[] = { (char*)"remEDIFIER" };
    int argc = 0;
    app = new QCoreApplication(argc, argv);
    qRegisterMetaType<bool>();
    thread = app->thread();
    return app->exec();
}
