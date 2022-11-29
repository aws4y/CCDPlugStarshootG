#include <stdio.h>
#include <stdlib.h>
#include "starshootg.h"

HStarshootg g_hcam = NULL;
void* g_pImageData = NULL;
unsigned g_total = 0;

static void __stdcall EventCallback(unsigned nEvent, void* pCallbackCtx)
{
    if (STARSHOOTG_EVENT_IMAGE == nEvent)
    {
        StarshootgFrameInfoV2 info = { 0 };
        HRESULT hr = Starshootg_PullImageV2(g_hcam, g_pImageData, 24, &info);
        if (FAILED(hr))
            printf("failed to pull image, hr = %08x\n", hr);
        else
        {
            /* After we get the image data, we can do anything for the data we want to do */
            printf("pull image ok, total = %u, res = %u x %u\n", ++g_total, info.width, info.height);
        }
    }
    else
    {
        printf("event callback: %d\n", nEvent);
    }
}

int main(int, char**)
{
    g_hcam = Starshootg_Open(NULL);
    if (NULL == g_hcam)
    {
        printf("no camera found or open failed\n");
        return -1;
    }
    
    int nWidth = 0, nHeight = 0;
    HRESULT hr = Starshootg_get_Size(g_hcam, &nWidth, &nHeight);
    if (FAILED(hr))
        printf("failed to get size, hr = %08x\n", hr);
    else
    {
        g_pImageData = malloc(TDIBWIDTHBYTES(24 * nWidth) * nHeight);
        if (NULL == g_pImageData)
            printf("failed to malloc\n");
        else
        {
            hr = Starshootg_StartPullModeWithCallback(g_hcam, EventCallback, NULL);
            if (FAILED(hr))
                printf("failed to start camera, hr = %08x\n", hr);
            else
            {
                printf("press ENTER to exit\n");
                getc(stdin);
            }
        }
    }
    
    /* cleanup */
    Starshootg_Close(g_hcam);
    if (g_pImageData)
        free(g_pImageData);
    return 0;
}
