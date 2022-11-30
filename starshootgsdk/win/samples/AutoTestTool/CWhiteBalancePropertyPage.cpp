#include "stdafx.h"
#include "global.h"
#include "AutoTestTool.h"
#include "CWhiteBalancePropertyPage.h"

CWhiteBalancePropertyPage::CWhiteBalancePropertyPage()
	: CPropertyPage(IDD_PROPERTY_WHITE_BALANCE)
{
}

void CWhiteBalancePropertyPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
}

LRESULT CWhiteBalancePropertyPage::OnWhiteBalance(WPARAM wp, LPARAM lp)
{
	int temp = 0, tint = 0;
	Starshootg_get_TempTint(g_hCam, &temp, &tint);
	SetTempValue(temp);
	SetTintValue(tint);
	return 0;
}

void CWhiteBalancePropertyPage::SetTempValue(int value)
{
	((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TEMP))->SetPos(value);
	SetDlgItemInt(IDC_STATIC_TEMP, value);
}

void CWhiteBalancePropertyPage::SetTintValue(int value)
{
	((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TINT))->SetPos(value);
	SetDlgItemInt(IDC_STATIC_TINT, value);
}

BEGIN_MESSAGE_MAP(CWhiteBalancePropertyPage, CPropertyPage)
	ON_WM_HSCROLL()
	ON_BN_CLICKED(IDC_BUTTON_WHITE_BALANCE, &CWhiteBalancePropertyPage::OnBnClickedButtonWhiteBalance)
	ON_MESSAGE(WM_USER_WHITE_BALANCE, &CWhiteBalancePropertyPage::OnWhiteBalance)
END_MESSAGE_MAP()

BOOL CWhiteBalancePropertyPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();

	((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TEMP))->SetRange(STARSHOOTG_TEMP_MIN, STARSHOOTG_TEMP_MAX);
	SetTempValue(STARSHOOTG_TEMP_DEF);
	((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TINT))->SetRange(STARSHOOTG_TINT_MIN, STARSHOOTG_TINT_MAX);
	SetTintValue(STARSHOOTG_TINT_DEF);

	return TRUE;  
}

void CWhiteBalancePropertyPage::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	int curTemp = 0, curTint = 0;
	if (pScrollBar == GetDlgItem(IDC_SLIDER_TEMP))
	{
		Starshootg_get_TempTint(g_hCam, &curTemp, &curTint);
		int temp = ((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TEMP))->GetPos();
		if (temp != curTemp)
		{
			Starshootg_put_TempTint(g_hCam, temp, curTint);
			SetDlgItemInt(IDC_STATIC_TEMP, temp);
		}
	}
	else if (pScrollBar == GetDlgItem(IDC_SLIDER_TINT))
	{
		Starshootg_get_TempTint(g_hCam, &curTemp, &curTint);
		int tint = ((CSliderCtrl*)GetDlgItem(IDC_SLIDER_TINT))->GetPos();
		if (tint != curTint)
		{
			Starshootg_put_TempTint(g_hCam, curTemp, tint);
			SetDlgItemInt(IDC_STATIC_TINT, tint);
		}
	}

	CPropertyPage::OnHScroll(nSBCode, nPos, pScrollBar);
}


void CWhiteBalancePropertyPage::OnBnClickedButtonWhiteBalance()
{
	Starshootg_AwbOnce(g_hCam, nullptr, nullptr);
}
