QT += bluetooth
QT -= gui

TEMPLATE = lib
CONFIG += c++17 unversioned_libname unversioned_soname
DESTDIR = ./build

DEFINES += APP_VERSION=\\\"$$VERSION\\\"
QMAKE_TARGET_PRODUCT = "libcomhelper"
QMAKE_TARGET_DESCRIPTION = "libcomhelper"
QMAKE_TARGET_COMPANY = "TheAirBlow"

copy_qt_libs.target = $$DESTDIR

win32 {
    copy_qt_libs.commands = cp $$[QT_INSTALL_BINS]/Qt5Core.dll $$[QT_INSTALL_BINS]/Qt5Bluetooth.dll $$DESTDIR/
}

# For this to work, LD_LIBRARY_PATH must be modified manually, which isn't a good solution.
# unix:!macx {
#     copy_qt_libs.commands = $$QMAKE_COPY $$[QT_INSTALL_LIBS]/libQt5Core.so $$DESTDIR/ && \
#                             $$QMAKE_COPY $$[QT_INSTALL_LIBS]/libQt5Bluetooth.so $$DESTDIR/
# }

macx {
    copy_qt_libs.commands = cp $$[QT_INSTALL_LIBS]/QtCore.framework/QtCore $$DESTDIR/libQt5Core.dylib && \
                            cp $$[QT_INSTALL_LIBS]/QtBluetooth.framework/QtBluetooth $$DESTDIR/libQt5Bluetooth.dylib
}

QMAKE_EXTRA_TARGETS += copy_qt_libs
QMAKE_POST_LINK += $$copy_qt_libs.commands

HEADERS += \
    ApplicationLoop.h \
    BluetoothAdapter.h \
    BluetoothClassic.h \
    BluetoothDiscovery.h \
    BluetoothLowEnergy.h

SOURCES += \
    ApplicationLoop.cpp \
    BluetoothAdapter.cpp \
    BluetoothClassic.cpp \
    BluetoothDiscovery.cpp \
    BluetoothLowEnergy.cpp
