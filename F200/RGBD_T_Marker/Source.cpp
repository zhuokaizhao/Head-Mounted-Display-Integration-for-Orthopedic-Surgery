// For Computer Integrated Surgery II
// This program provides the transformation result from the paper marker to F200
// Program uses ARToolKit
// Author: Zhuokai Zhao
// Contact: zzhao30@jhu.edu

#include <stdio.h>
#include <string.h>

#ifdef _WIN32
#	define snprintf _snprintf
#endif

#include <stdlib.h>					// malloc(), free()
#ifdef __APPLE__
#	include <GLUT/glut.h>
#else
#	include <GL/glut.h>
#endif

#include <AR/config.h>
#include <AR/video.h>
#include <AR/param.h>			// arParamDisp()
#include <AR/ar.h>
#include <AR/gsub_lite.h>

// Global Variables

// Image acquisition
static ARUint8 *gARTImage = NULL;

// Marker detection
static ARHandle *gARHandle = NULL;
static ARPattHandle	*gARPattHandle = NULL;

// Calculate transformation
static AR3DHandle *gAR3DHandle = NULL;
static ARdouble gPatt_width = 80.0;
static ARdouble gPatt_trans[3][4];
static int gPatt_found = FALSE;
static int gPatt_id;

// *****************************************************************************
// Part I, Initialize the video grabbing from the camera and load the marker
// *****************************************************************************

// Helper functions: setupCamera and setupMarker
// setupCamera loads camera calibration results, opens a connection to the camera and 
// records the camera settings into 3 variables which are passed in as parameters.

static int setupCamera(const char *cparam_name, char *vconf, ARHandle **arhandle, AR3DHandle **ar3dhandle) {
	
	// size of the window
	int xsize, ysize;
	// Camera parameters
	ARParam cparam;
	// Format of the returned pixel
	AR_PIXEL_FORMAT pixFormat;

	// Step 1: Open the video path
	arVideoOpen(vconf);

	// Step 2: Get the size of the window
	arVideoGetSize(&xsize, &ysize);

	// Step 3: Get the format of the returned pixels
	pixFormat = arVideoGetPixelFormat();

	// Step 4: Load the camera parameters
	arParamLoad(cparam_name, 1, &cparam);

	// Step 5: Resize the window if needed and initialize
	if (cparam.xsize != xsize || cparam.ysize != ysize)
	{
		arParamChangeSize(&cparam, xsize, ysize, &cparam);
	}

	// Step 6: Set up defaults related to the tracking part
	//*cparamLT_p = arParamLTCreate(&cparam, AR_PARAM_LT_DEFAULT_OFFSET);
	//*arhandle = arCreateHandle(*cparamLT_p);
	//arSetPixelFormat(*arhandle, pixFormat);
	//*ar3dhandle = ar3DCreateHandle(&cparam);

	// Step 7: Start the video
	arVideoCapStart();

	return (TRUE);
}

// setupMarker loads pattern files for the patterns we want to detect. 

static int setupMarker(const char *patt_name, int *patt_id, ARHandle *arhandle, ARPattHandle **pattHandle_p) {
	// Load pattern handle
	*pattHandle_p = arPattCreateHandle();

	// Load Hiro pattern
	*patt_id = arPattLoad(*pattHandle_p, patt_name);

	// Connect the handle and the pattern
	arPattAttach(arhandle, *pattHandle_p);

	return (TRUE);
}
// Helper Function calTran calculates the camera transformation
// *****************************************************************************
// Part II, Grab a video input frame
// *****************************************************************************

// *****************************************************************************
// Part III, Detect the markers
// *****************************************************************************

// *****************************************************************************
// Part IV, Calculate camera transformation (what we want from this program)
// *****************************************************************************

static void calTran(void) {
	// current and previous time since the program starts
	int ms;
	static int ms_prev;
	// elaspsed time
	float s_elapsed;
	// Current grabbed image
	ARUint8 *image;
	// Transformation error
	ARdouble err;

	// Set the update frequency, no more often than 100Hz
	ms = glutGet(GLUT_ELAPSED_TIME);
	s_elapsed = (float)(ms - ms_prev) * 0.001f;
	if (s_elapsed < 0.01f)
		return;
	ms_prev = ms;

	// Grab a video frame
	image = arVideoGetImage();
	if (image != NULL)
	{
		// if not NULL, save
		gARTImage = image;
	}

	// Detect the marker in the grabbed frame
	arDetectMarker(gARHandle, gARTImage);

	// Find the marker that matches the preferred pattern
	int k = -1;
	for (int j = 0; j < gARHandle->marker_num; j++)
	{
		if (gARHandle->markerInfo[j].id == gPatt_id)
		{
			if (k == -1)
				k = j;
			else if (gARHandle->markerInfo[j].cf > gARHandle->markerInfo[k].cf)
				k = j;
		}
	}

	// Calculate the transformation between the marker and the camera
	if (k != -1)
	{
		err = arGetTransMatSquare(gAR3DHandle, &(gARHandle->markerInfo[k]), gPatt_width, gPatt_trans);
		gPatt_found = TRUE;
	}
	else
	{
		gPatt_found = FALSE;
	}

	// Update GLUT
	glutPostRedisplay();
}

// main function
int main(int argc, char** argv) {

	char glutGamemode[32];
	// Video configuration string
	char vconf[] = "";
	// F200 Calibration Result
	char *cparam_name = "Data/camera_para.dat";
	// Marker used
	char patt_name[] = "Data/hiro.patt";

	// Initialize library
	glutInit(&argc, argv);

	// Set up camera for the video
	setupCamera(cparam_name, vconf, &gARHandle, &gAR3DHandle);

	// Set up markers
	setupMarker(patt_name, &gPatt_id, gARHandle, &gARPattHandle);

	// Find the transformation
	glutIdleFunc(calTran);
}

