Head-Mounted Display Integration for Orthopedic Surgery
========================================================================================
Contributer: Zhuokai Zhao

.. begin_brief_description

Orthopedic surgery has been very commonly conducted these years, yet it suffers from the lack of efficient harmless guidance system. Current guidance system uses X-rays to only provide the images without any tool-tracking. It starts with acquiring multiple X-ray images from different views to locate the point of entry, under the help of a reference tool. The medical instrument is then invaded and moved inside the patient’s body with small displacements. A set of anteroposterior X-ray images are acquired during each small displacement, until the target position is reached. The current workflow is harmful and inefficient. It requires numerous X-ray images for placing wires and screws, which not only harms the patient and surgeon in a direct way, but also increases the probability of potentially damaging the patient’s soft tissues and nervous system. 

The project focuses on using augmented reality to visualize the occluded part of the needle in HoloLens, which is less time-consuming, more efficient and prevents the frequent use of 2D X-rays. The whole process also requires tracking the needle position and estimating the needle tip location. The diagram for the system set-up is showed in the graph below. The main hardwares needed for this project are, a RGBD Camera (Intel RealSense F200) and a HoloLens.

.. image:: https://github.com/zhuokaizhao/Head-Mounted-Display-Integration-for-Orthopedic-Surgery/blob/master/Images/System_Setup.jpg
   :alt: System set up
   :align: center


.. contents:: Contents
   :local:
   :backlinks: none


Unity
----------------------------------------------------------------------------------------
.. begin_detailed_description	
Contains Unity applications that run on HoloLens

* /Unity/HoloLensOrthopedic is the demo app that runs in HoloLens. Detailed instruction on how to run Unity application on HoloLens could be found here_

.. _here: https://docs.microsoft.com/en-us/windows/mixed-reality/unity-development-overview

* /Unity/RGBD_to_HoloLens is another Unity application that performs pivot calibration and saves marker-to-camera transformations. There is an option in the source code that could be changed to whether or not save the transformation results. If the saving option is true, the transformations will be saved to /F200/Pivot Calibration/pointTipNeedle.txt. Notice that the old files should be deleted manually before saving new results.
		

F200
----------------------------------------------------------------------------------------
.. begin_detailed_description
* After the marker-to-camera transformations result is saved, pivot calibration result could be computed by Python scripts located at /F200/Pivot Calibration/pivot.py. Running this source code with terminal will automatically create a file called "pointTip.txt", which is the tool tip position with respect to the marker coordinate.  


* /F200/RGBD_T_Marker is a PC-running copy that performs the same procedure as /Unity/RGBD_to_HoloLens.


* /F200/CheckAccuracy calculates the end-to-end combined error of camera calibration and pivot calibration.


Paper
----------------------------------------------------------------------------------------
.. begin_detailed_description
PDF version of the report which includes all the details about the algorithm and code.


Demo
----------------------------------------------------------------------------------------
* Tracking performed within HoloLens:

.. image:: https://github.com/zhuokaizhao/Head-Mounted-Display-Integration-for-Orthopedic-Surgery/blob/master/Demo/hololens_tracking.png
   :alt: tracking
   :align: center


* Display the occuluded part of the surgical tool:

.. image:: https://github.com/zhuokaizhao/Head-Mounted-Display-Integration-for-Orthopedic-Surgery/blob/master/Demo/inserted1.jpg
   :alt: inserted1
   :align: center

.. image:: https://github.com/zhuokaizhao/Head-Mounted-Display-Integration-for-Orthopedic-Surgery/blob/master/Demo/inserted2.jpg
   :alt: inserted2
   :align: center




