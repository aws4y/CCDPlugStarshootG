#include "mainwindow.h"
#include <QToolBar>
#include <QStatusBar>
#include <QString>
#include <QGroupBox>
#include <QBoxLayout>

#define COUNT_TIME_RANGE 500000

MainWindow::MainWindow(QWidget* parent)
    : QMainWindow(parent)
    , m_hStarshootg(nullptr), m_pData(nullptr)
    , m_resLevel(0), m_frame(0), m_fps(0)
    , m_temperature(STARSHOOTG_TEMP_DEF), m_tint(STARSHOOTG_TINT_DEF)
    , m_imgWidth(0), m_imgHeight(0)
{
    setMinimumSize(1024, 768);
    QGroupBox* gbox_device = new QGroupBox("Device");
    m_cmb_camera = new QComboBox;
    QVBoxLayout* vlyt_dev = new QVBoxLayout;
    vlyt_dev->addWidget(m_cmb_camera);
    gbox_device->setLayout(vlyt_dev);
    connect(m_cmb_camera, SIGNAL(currentIndexChanged(int)), this, SLOT(onCameraChanged(int)));

    QGroupBox* gbox_res = new QGroupBox("Resolution");
    m_cmb_resolution = new QComboBox;
    QVBoxLayout* vlyt_res = new QVBoxLayout;
    vlyt_res->addWidget(m_cmb_resolution);
    gbox_res->setLayout(vlyt_res);
    connect(m_cmb_resolution, SIGNAL(currentIndexChanged(int)), this, SLOT(onResolutionChanged(int)));

    QGroupBox* gbox_exp = new QGroupBox("Exposure");
    m_cbox_auto = new QCheckBox;
    m_cbox_auto->setEnabled(false);
    m_cbox_auto->setCheckState(Qt::CheckState::Checked);
    QLabel* lbl_auto = new QLabel("Auto exposure");
    QHBoxLayout* hlyt_auto = new QHBoxLayout;
    hlyt_auto->addWidget(m_cbox_auto);
    hlyt_auto->addSpacing(10);
    hlyt_auto->addWidget(lbl_auto);
    hlyt_auto->addStretch();
    QLabel* lbl_time = new QLabel("Time(us):");
    QLabel* lbl_gain = new QLabel("Gain(%):");
    m_lbl_expoTime = new QLabel("0");
    m_lbl_expoGain = new QLabel("0");
    m_slider_expoTime = new QSlider(Qt::Horizontal);
    m_slider_expoGain = new QSlider(Qt::Horizontal);

    m_slider_expoTime->setEnabled(false);
    m_slider_expoGain->setEnabled(false);
    QVBoxLayout* glyt_exp = makeVLyt(lbl_time, m_slider_expoTime, m_lbl_expoTime, lbl_gain, m_slider_expoGain, m_lbl_expoGain);
    QVBoxLayout* vlyt_exp = new QVBoxLayout;
    vlyt_exp->addLayout(hlyt_auto);
    vlyt_exp->addLayout(glyt_exp);
    gbox_exp->setLayout(vlyt_exp);
    connect(m_cbox_auto, &QCheckBox::stateChanged, this, &MainWindow::onAutoExpo);
    connect(m_slider_expoTime, &QSlider::valueChanged, this, &MainWindow::onExpoTime);
    connect(m_slider_expoGain, &QSlider::valueChanged, this, &MainWindow::onExpoGain);

    QGroupBox* gbox_wb = new QGroupBox("White balance");
    m_btn_autoWB = new QPushButton("White balance");
    m_btn_autoWB->setEnabled(false);
    connect(m_btn_autoWB, &QPushButton::clicked, this, &MainWindow::onAutoWB);
    QLabel* lbl_temp = new QLabel("Temperature:");
    QLabel* lbl_tint = new QLabel("Tint:");
    m_lbl_temperature = new QLabel(QString::number(STARSHOOTG_TEMP_DEF));
    m_lbl_tint = new QLabel(QString::number(STARSHOOTG_TINT_DEF));
    m_slider_temperature = new QSlider(Qt::Horizontal);
    m_slider_tint = new QSlider(Qt::Horizontal);
    m_slider_temperature->setRange(STARSHOOTG_TEMP_MIN, STARSHOOTG_TEMP_MAX);
    m_slider_temperature->setValue(STARSHOOTG_TEMP_DEF);
    m_slider_tint->setRange(STARSHOOTG_TINT_MIN, STARSHOOTG_TINT_MAX);
    m_slider_tint->setValue(STARSHOOTG_TINT_DEF);
    m_slider_temperature->setEnabled(false);
    m_slider_tint->setEnabled(false);
    QVBoxLayout* glyt_wb = makeVLyt(lbl_temp, m_slider_temperature, m_lbl_temperature, lbl_tint, m_slider_tint, m_lbl_tint);
    QVBoxLayout* vlyt_wb = new QVBoxLayout;
    vlyt_wb->addLayout(glyt_wb);
    gbox_wb->setLayout(vlyt_wb);
    vlyt_wb->addWidget(m_btn_autoWB);
    connect(m_slider_temperature, &QSlider::valueChanged, this, &MainWindow::onWBTemp);
    connect(m_slider_tint, &QSlider::valueChanged, this, &MainWindow::onWBTint);

    m_btn_open = new QPushButton("Open");
    m_btn_open->setEnabled(false);
    connect(m_btn_open, &QPushButton::clicked, this, &MainWindow::onBtnOpen);

    QVBoxLayout* vlyt_ctrl = new QVBoxLayout;
    vlyt_ctrl->addWidget(gbox_device);
    vlyt_ctrl->addWidget(gbox_res);
    vlyt_ctrl->addWidget(gbox_exp);
    vlyt_ctrl->addWidget(gbox_wb);
    vlyt_ctrl->addStretch(1);
    vlyt_ctrl->addWidget(m_btn_open);
    QWidget* wg_ctrl = new QWidget;
    wg_ctrl->setLayout(vlyt_ctrl);
    wg_ctrl->setFixedWidth(300);

    m_lbl_frame = new QLabel;
    QHBoxLayout* hlyt_frame = new QHBoxLayout;
    hlyt_frame->addStretch();
    hlyt_frame->addWidget(m_lbl_frame);
    hlyt_frame->setMargin(0);
    m_lbl_video = new QLabel;
    QHBoxLayout* hlyt_show = new QHBoxLayout;
    hlyt_show->addStretch();
    hlyt_show->addWidget(m_lbl_video);
    hlyt_show->addStretch();
    QVBoxLayout* vlyt_show = new QVBoxLayout;
    vlyt_show->addStretch();
    vlyt_show->addLayout(hlyt_show);
    vlyt_show->addStretch();
    vlyt_show->addLayout(hlyt_frame);
    QWidget* wg_show = new QWidget;
    wg_show->setLayout(vlyt_show);

    QHBoxLayout* hlyt_main = new QHBoxLayout;
    hlyt_main->addWidget(wg_ctrl);
    hlyt_main->addWidget(wg_show);
    QWidget* w_main = new QWidget;
    w_main->setLayout(hlyt_main);
    setCentralWidget(w_main);

    m_millisecond = new QTimer;
    int ret = Starshootg_Enum(m_allDevs);
    if (ret > 0)
    {
        for (int i = 0; i < ret; ++i)
            m_cmb_camera->addItem(QString::fromWCharArray(m_allDevs[i].displayname));
        m_btn_open->setEnabled(true);
        connect(this, &MainWindow::transImageInfo, this, &MainWindow::onTransImageInfo);
    }
}

void MainWindow::closeEvent(QCloseEvent*)
{
    if (m_hStarshootg)
    {
        Starshootg_Close(m_hStarshootg);
        m_hStarshootg = nullptr;
        delete[] m_pData;
        m_pData = nullptr;
    }
}

void MainWindow::onCameraChanged(int index)
{
    m_cmb_resolution->clear();
    m_curDev = m_allDevs[index];
    for (int i = 0; i < m_curDev.model->preview; ++i)
    {
        QString str = QString("%1*%2").arg(m_curDev.model->res[i].width, 0, 10).arg(m_curDev.model->res[i].height, 0, 10);
        m_cmb_resolution->addItem(str);
    }
}

void MainWindow::onResolutionChanged(int index)
{
    m_resLevel = index;
    m_imgWidth = m_curDev.model->res[index].width;
    m_imgHeight = m_curDev.model->res[index].height;
}

void MainWindow::onAutoExpo(bool state)
{
    if (!m_hStarshootg)
        return;

    if (Starshootg_put_AutoExpoEnable(m_hStarshootg, state) < 0)
        return;

    m_slider_expoTime->setEnabled(!state);
    m_slider_expoGain->setEnabled(!state);
}

void MainWindow::onExpoTime(int value)
{
    if (!m_hStarshootg)
        return;
    m_lbl_expoTime->setText(QString::number(value));
    if (!m_cbox_auto->isChecked())
       Starshootg_put_ExpoTime(m_hStarshootg, value);
}

void MainWindow::onExpoGain(int value)
{
    if (!m_hStarshootg)
        return;
    m_lbl_expoGain->setText(QString::number(value));

    if (!m_cbox_auto->isChecked())
        Starshootg_put_ExpoAGain(m_hStarshootg, value);
}

void MainWindow::onAutoWB()
{
    if (!m_hStarshootg)
        return;
    Starshootg_AwbOnePush(m_hStarshootg, wbCallback, this);
}

void MainWindow::wbCallback(const int nTemp, const int nTint, void *pCtx)
{
    MainWindow* pThis = static_cast<MainWindow*>(pCtx);
    pThis->m_slider_temperature->setValue(nTemp);
    pThis->m_slider_tint->setValue(nTint);
}

void MainWindow::onWBTemp(int value)
{
    if (!m_hStarshootg || Starshootg_put_TempTint(m_hStarshootg, m_temperature, m_tint) < 0)
        return;
    m_lbl_temperature->setText(QString::number(value));
    m_temperature = value;
}

void MainWindow::onWBTint(int value)
{
    if (!m_hStarshootg || Starshootg_put_TempTint(m_hStarshootg, m_temperature, m_tint) < 0)
        return;
    m_lbl_tint->setText(QString::number(value));
    m_tint = value;
}

void MainWindow::onBtnOpen()
{
    if (m_hStarshootg)
    {
        Starshootg_Close(m_hStarshootg);
        m_hStarshootg = nullptr;
        delete[] m_pData;
		m_pData = nullptr;
		
        m_btn_open->setText("Open");
        m_millisecond->stop();
        m_frame = 0;
        m_fps = 0;
        m_lbl_frame->clear();
        m_cmb_camera->setEnabled(true);
        m_cmb_resolution->setEnabled(true);
        m_cbox_auto->setCheckState(Qt::CheckState::Checked);
        m_cbox_auto->setEnabled(false);
        m_slider_expoGain->setEnabled(false);
        m_slider_expoTime->setEnabled(false);
        m_btn_autoWB->setEnabled(false);
        m_slider_temperature->setEnabled(false);
        m_slider_tint->setEnabled(false);
    }
	else
    {
        m_hStarshootg = Starshootg_Open(m_curDev.id);
        if (m_hStarshootg)
        {
            Starshootg_put_Speed(m_hStarshootg, 1);
            Starshootg_put_eSize(m_hStarshootg, static_cast<unsigned>(m_resLevel));
            m_pData = new uchar[TDIBWIDTHBYTES(m_imgWidth * 24) * m_imgHeight];
            unsigned uimax = 0,  uimin = 0, uidef = 0;
            unsigned short usmax = 0, usmin = 0, usdef = 0;
            Starshootg_get_ExpTimeRange(m_hStarshootg, &uimin, &uimax, &uidef);
            m_slider_expoTime->setRange(uimin, uimax);
            m_slider_expoTime->setValue(uidef);
            Starshootg_get_ExpoAGainRange(m_hStarshootg, &usmin, &usmax, &usdef);
            m_slider_expoGain->setRange(usmin, usmax);
            m_slider_expoGain->setValue(usdef);
            Starshootg_put_AutoExpoEnable(m_hStarshootg, true);
            if (Starshootg_StartPullModeWithCallback(m_hStarshootg, eventCallBack, this) >= 0)
            {
                m_millisecond->start(COUNT_TIME_RANGE);
                m_cmb_resolution->setEnabled(false);
                m_cmb_camera->setEnabled(false);
                m_cbox_auto->setEnabled(true);
                m_btn_autoWB->setEnabled(true);
                m_slider_temperature->setEnabled(true);
                m_slider_tint->setEnabled(true);
                m_btn_open->setText("Close");
            }
            else
            {
                Starshootg_Close(m_hStarshootg);
                m_hStarshootg = nullptr;
                delete[] m_pData;
				m_pData = nullptr;
            }
        }
    }
}

void MainWindow::eventCallBack(unsigned nEvent, void* pCallbackCtx)
{
    MainWindow* pThis = static_cast<MainWindow*>(pCallbackCtx);
    if (STARSHOOTG_EVENT_IMAGE == nEvent)
        pThis->handleImageCallback();
    else if (STARSHOOTG_EVENT_EXPOSURE == nEvent)
        pThis->handleExpCallback();
}

void MainWindow::handleImageCallback()
{
    unsigned width = 0, height = 0;
    if (Starshootg_PullImage(m_hStarshootg, m_pData, 24, &width, &height) >= 0)
    {
        QImage image(m_pData, width, height, QImage::Format_RGB888);
        emit transImageInfo(image);
    }
}

void MainWindow::handleExpCallback()
{
    if (m_cbox_auto->isChecked())
    {
        unsigned time = 0;
        unsigned short gain = 0;
        Starshootg_get_ExpoTime(m_hStarshootg, &time);
        Starshootg_get_ExpoAGain(m_hStarshootg, &gain);
        m_slider_expoTime->setValue(int(time));
        m_slider_expoGain->setValue(int(gain));
    }
}

void MainWindow::onTransImageInfo(QImage image)
{
    if (m_frame == 5)
    {
        const double interval = COUNT_TIME_RANGE - m_millisecond->remainingTime();
        m_fps = m_frame / interval * 1000;
        m_frame = 0;
        m_millisecond->start(COUNT_TIME_RANGE);
    }
    m_lbl_frame->setText("FPS: " + QString::number(m_fps, 10, 1));
    m_frame++;
    QImage img = image.scaled(this->width() - 500, this->height() - 200, Qt::KeepAspectRatio, Qt::FastTransformation);
    m_lbl_video->setPixmap(QPixmap::fromImage(img));
}


QVBoxLayout* MainWindow::makeVLyt(QLabel* lbl_1, QSlider* sli_1, QLabel* val_1, QLabel* lbl_2, QSlider* sli_2, QLabel* val_2)
{
    QHBoxLayout* hlyt_1 = new QHBoxLayout;
    hlyt_1->addWidget(lbl_1);
    hlyt_1->addStretch();
    hlyt_1->addWidget(val_1);
    hlyt_1->setMargin(0);
    QHBoxLayout* hlyt_2 = new QHBoxLayout;
    hlyt_2->addWidget(lbl_2);
    hlyt_2->addStretch();
    hlyt_2->addWidget(val_2);
    hlyt_2->setMargin(0);
    QVBoxLayout* vlyt = new QVBoxLayout;
    vlyt->addLayout(hlyt_1);
    vlyt->addWidget(sli_1);
    vlyt->addSpacing(5);
    vlyt->addLayout(hlyt_2);
    vlyt->addWidget(sli_2);
    return vlyt;
}
