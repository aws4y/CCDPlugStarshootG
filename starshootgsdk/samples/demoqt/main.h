#ifndef __MAIN_H__
#define __MAIN_H__

#include <QMainWindow>
#include <QPushButton>
#include <QComboBox>
#include <QLabel>
#include <QTimer>
#include <QCheckBox>
#include <QVBoxLayout>
#include <QSlider>
#include <starshootg.h>

class MainWindow : public QMainWindow
{
    Q_OBJECT
    StarshootgDevice  m_allDevs[STARSHOOTG_MAX];
    StarshootgDevice  m_curDev;
    HStarshootg       m_hStarshootg;
    QComboBox*     m_cmb_camera;
    QComboBox*     m_cmb_resolution;
    QCheckBox*     m_cbox_auto;
    QSlider*       m_slider_expoTime;
    QSlider*       m_slider_expoGain;
    QSlider*       m_slider_temperature;
    QSlider*       m_slider_tint;
    QLabel*        m_lbl_expoTime;
    QLabel*        m_lbl_expoGain;
    QLabel*        m_lbl_temperature;
    QLabel*        m_lbl_tint;
    QLabel*        m_lbl_video;
    QLabel*        m_lbl_frame;
    QPushButton*   m_btn_autoWB;
    QPushButton*   m_btn_open;
    QTimer*        m_millisecond;
    unsigned       m_imgWidth;
    unsigned       m_imgHeight;
    uchar*         m_pData;
    int            m_resLevel;
    int            m_frame;
    double         m_fps;
    short int      m_temperature;
    short int      m_tint;
public:
    MainWindow(QWidget* parent = nullptr);
protected:
    void closeEvent(QCloseEvent*) override;
private:
    static void eventCallBack(unsigned nEvent, void* pCallbackCtx);
    static void wbCallback(const int nTemp, const int nTint, void* pCtx);
    void handleImageCallback();
    void handleExpCallback();
    QVBoxLayout* makeVLyt(QLabel*, QSlider*, QLabel*, QLabel*, QSlider*, QLabel*);
signals:
    void transImageInfo(QImage image);
private slots:
    void onCameraChanged(int);
    void onResolutionChanged(int);
    void onAutoExpo(bool);
    void onExpoTime(int);
    void onExpoGain(int);
    void onAutoWB();
    void onWBTemp(int);
    void onWBTint(int);
    void onBtnOpen();
    void onTransImageInfo(QImage image);
};

#endif
