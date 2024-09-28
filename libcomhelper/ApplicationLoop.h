#ifndef APPLICATIONLOOP_H
#define APPLICATIONLOOP_H

#include <QBluetoothDeviceDiscoveryAgent>
#include <QCoreApplication>

QCoreApplication* GetApplication();
QThread* GetMainThread();

extern "C" {
void ExitApplication(int code);
int RunApplication();
}

#endif // APPLICATIONLOOP_H
