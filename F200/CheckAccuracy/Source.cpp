//
//  source.cpp
//  singleCamera_Calibration
//
//  Created by Zhuokai Zhao on 2/12/17.
//  Copyright © 2017 Zhuokai Zhao. All rights reserved.
//

#include <stdio.h>
#include <iostream>
#include <string>
#include <vector>

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/calib3d.hpp>

// Intel RealSense 
#include <librealsense/rs.hpp>

using namespace cv;
using namespace std;
using namespace rs;

vector< vector<Point2f> > allCorners;
vector< vector<Point3f> > allObjects;
Size img_size;

// Define window size and frame rate used by visualization
int const INPUT_WIDTH = 640;
int const INPUT_HEIGHT = 480;
int const FRAMERATE = 60;

// Define windows names
char* const WINDOW_RGB = "RGB Image Streaming";

// Global variables that are used in multiple functions
context 	rsContext;
device* 	rsCamera = NULL;
intrinsics 	intrinIR;
intrinsics  intrinRGB;
bool 		isContinue = true;
const float PI = 3.1415926;
Mat curRGB;

// Initialize the application state. Upon success will return the static app_state vars address
bool initialize_streaming()
{
	if (rsContext.get_device_count() > 0)
	{
		// get the camera device, default device number is 0 if only one camera is connected
		rsCamera = rsContext.get_device(0);

		// stream RGB data
		rsCamera->enable_stream(rs::stream::color, INPUT_WIDTH, INPUT_HEIGHT, rs::format::rgb8, FRAMERATE);

		// start
		rsCamera->start();

		return true;
	}

	return false;
}

// stop the streaming and close the windows if was clicked on either image
static void onMouse(int event, int x, int y, int, void* window_name)
{
	if (event == cv::EVENT_LBUTTONDOWN)
	{
		// Get current frames intrinsic data.
		intrinRGB = rsCamera->get_stream_intrinsics(rs::stream::color);
		//cout << intrinRGB.width << ", " << intrinRGB.height << endl;
		// Create color image
		cv::Mat rgb(intrinRGB.height, intrinRGB.width, CV_8UC3, (uchar *)rsCamera->get_frame_data(rs::stream::color));

		// convert the data from OpenCV's BGR to RGB
		cv::cvtColor(rgb, rgb, cv::COLOR_BGR2RGB);

		curRGB = rgb;
		// cout << curRGB.size().width << ", " << curRGB.size().height << endl;
		//cv::imshow("aaa", curRGB);
		isContinue = false;
	}
}

// initialize RGB windowS for showing the streaming, has the function to close them with mouse-click
void setup_windows()
{
	cv::namedWindow(WINDOW_RGB, 1);
	cv::setMouseCallback(WINDOW_RGB, onMouse, WINDOW_RGB);
}

// Called every frame gets RGB data from streams and displays using OpenCV.
void display_next_frame_RGB()
{
	// Get current frames intrinsic data.
	intrinRGB = rsCamera->get_stream_intrinsics(rs::stream::color);

	// Create color image
	cv::Mat rgb(intrinRGB.height, intrinRGB.width, CV_8UC3, (uchar *)rsCamera->get_frame_data(rs::stream::color));

	// convert the data from OpenCV's BGR to RGB
	cv::cvtColor(rgb, rgb, cv::COLOR_BGR2RGB);
	imshow(WINDOW_RGB, rgb);
	cvWaitKey(1);
}

// helper functions for loading images and finish system set-up
void calibPreparation(Size patternSize, int squareSize, Mat curImage) {
	// imshow("aaa", curImage);
	// Initialize variables for later use
	Mat curGrayImage;
	// curCorners contains 2D points/corners showed on the current image
	vector<Point2f> curCorners;
	// curObjects contains 3D points of the corresponding points on current objects
	vector<Point3f> curObjects;

	// Generate the corresponding 3D locations of the current object
	for (int i = 0; i < patternSize.height; i++)
	{
		for (int j = 0; j < patternSize.width; j++)
		{
			curObjects.push_back(Point3f((float)j*squareSize, (float)i*squareSize, 0));
		}
	}

	// Load images
	img_size = curImage.size();

	// Convert the image to grey image for better result
	cvtColor(curImage, curGrayImage, CV_BGR2GRAY);

	// Find the positions of internal corners of the checkerboard
	bool found;
	found = findChessboardCorners(curGrayImage, patternSize, curCorners, CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
	cout << found << endl;
	// If done with success
	if (found)
	{
		cornerSubPix(curGrayImage, curCorners, Size(7, 7), Size(-1, -1), TermCriteria(TermCriteria::EPS + TermCriteria::COUNT, 30, 0.1));

		// push the found corners
		allCorners.push_back(curCorners);
		allObjects.push_back(curObjects);
	}

}

int main(int argc, const char * argv[]) {

	// Show the openCV version (I am using Version 3.2.0 on my laptop)
	std::cout << "OpenCV Version " << CV_VERSION << std::endl;

	// Initialize streaming
	if (!initialize_streaming())
	{
		std::cout << "Unable to locate a camera" << std::endl;
		return -1;
	}

	// Set up display windows
	setup_windows();

	while (isContinue)
	{
		if (rsCamera->is_streaming())
			rsCamera->wait_for_frames();

		// stream the data
		display_next_frame_RGB();	
	}

	// Define the size of the checkerboard
	Size patternSize;
	patternSize.width = 5;
	patternSize.height = 7;

	// Square size is 30 mm
	int squareSize = 30;

	// Call the preparation helper function
	calibPreparation(patternSize, squareSize, curRGB);

	// Now start the calibration process as we have the extrated corner locations
	// Intrinsic matrix K
	Mat K;
	// Distortion Coefficient D
	Mat D;
	// Rotation and Translation vectors of each view
	vector< Mat > rvecs, tvecs;
	// Standard deviations estimated for intrinsic parameters
	Mat stdDeviationsIntrinsics;
	// Standard deviations estimated for extrinsic parameters
	Mat stdDeviationsExtrinsics;
	// RMS re-projection error estimated for each pattern view
	Mat perViewErrors;

	double rms;
	rms = calibrateCamera(allObjects, allCorners, img_size, K, D, rvecs, tvecs, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors, 0);

	// Print out the result
	//cout << "rvec is: " << endl;
	//cout << rvecs[0] << endl;
	//cout << "tvec is: " << endl;
	//cout << tvecs[0] << endl;

	cout << "Re-projection Error is: " << endl;
	cout << rms << endl;

	// convert the rvec to rotation matrix
	Mat R, t;
	Rodrigues(rvecs[0], R);
	// convert the matrix to float
	R.convertTo(R, CV_32FC1);
	t = tvecs[0];
	t.convertTo(t, CV_32FC1);

	//cout << "Rotation Matrix is: " << endl;
	//cout << R << endl;
	//cout << "Translation Matrix is: " << endl;
	//cout << t << endl;
	
	// Now we convert the points 3D position in terms of chessboard to be with respect with camera
	vector<Mat> convertedPositions;
	for (int i = 0; i < allObjects[0].size(); i++)
	{
		// construct 3*1 matrix
		Mat curPosition;
		curPosition = (Mat_<float>(3, 1) << allObjects[0][i].x,
											allObjects[0][i].y,
											allObjects[0][i].z);

		Mat curNewPosition;
		curNewPosition = R * curPosition + t;
		// transpose to 1*3 to make save/print convenient
		transpose(curNewPosition, curNewPosition);
		convertedPositions.push_back(curNewPosition);
	}

	// show the converted positions
	/*
	cout << "All the converted positions are: " << endl;
	for (int i = 0; i < convertedPositions.size(); i++)
	{
		cout << convertedPositions[i] << endl;
	}
	*/
	
	// save the result to txt file
	char name[] = "C:/Users/zhaoz/Desktop/JohnsHopkins/Spring2017/CISII/F200/CheckAccuracy/Checkerboard Corners.txt";
	FILE* pFile = fopen(name, "r");
	// if the txt file does not exist
	if (pFile == NULL)
	{
		// create a new txt file
		ofstream outFile(name);
		for (int i = 0; i < convertedPositions.size(); i++)
		{
			for (int j = 0; j < 3; j++)
			{
				outFile << convertedPositions[i].at<float>(0, j) << " ";
			}
			outFile << "\n";
		}
	}
	else
	{
		// remove the old one
		remove(name);
		// create a new txt file
		ofstream outFile(name);
		for (int i = 0; i < convertedPositions.size(); i++)
		{
			for (int j = 0; j < 3; j++)
			{
				outFile << convertedPositions[i].at<float>(0, j) << " ";
			}
			outFile << "\n";
		}
	}
	
	return 0;
}