#include "stdafx.h"
#include "global.h"
#include "AutoTestTool.h"
#include "CBitDepthPropertyPage.h"

CBitDepthPropertyPage::CBitDepthPropertyPage()
	: CPropertyPage(IDD_PROPERTY_BITDEPTH)
	, m_bitDepth(FALSE)
{
}

void CBitDepthPropertyPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Radio(pDX, IDC_RADIO_8_BIT, m_bitDepth);
}

BEGIN_MESSAGE_MAP(CBitDepthPropertyPage, CPropertyPage)
	ON_BN_CLICKED(IDC_RADIO_8_BIT, &CBitDepthPropertyPage::OnBnClickedRadio8Bit)
	ON_BN_CLICKED(IDC_RADIO_HIGH_BIT, &CBitDepthPropertyPage::OnBnClickedRadioHighBit)
END_MESSAGE_MAP()

BOOL CBitDepthPropertyPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();

	if (g_hCam)
	{
		int maxBit = Starshootg_get_MaxBitDepth(g_hCam);
		if (maxBit <= 8)
			GetDlgItem(IDC_RADIO_HIGH_BIT)->ShowWindow(FALSE);
		else
		{
			GetDlgItem(IDC_RADIO_HIGH_BIT)->ShowWindow(TRUE);
			CString text;
			text.Format(_T("%d bits"), maxBit);
			SetDlgItemText(IDC_RADIO_HIGH_BIT, text);
		}
		Starshootg_get_Option(g_hCam, STARSHOOTG_OPTION_BITDEPTH, &m_bitDepth);
		UpdateData(FALSE);
	}

	return TRUE;
}

void CBitDepthPropertyPage::OnBnClickedRadio8Bit()
{
	UpdateData(TRUE);
	Starshootg_put_Option(g_hCam, STARSHOOTG_OPTION_BITDEPTH, m_bitDepth);
}

void CBitDepthPropertyPage::OnBnClickedRadioHighBit()
{
	UpdateData(TRUE);
	Starshootg_put_Option(g_hCam, STARSHOOTG_OPTION_BITDEPTH, m_bitDepth);
}
