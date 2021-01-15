# Fingertip Touch Interface - Unity Simulation

This Unity project is used for the simulating the relationship between value of finger joints and the captured images from Raspberry Pi camera attached to the thumb.

This repo is related to the repo [
2D-Continuous-Thumb-Pinch-and-Sliding-Tracking ](https://github.com/BerwinZ/2D-Continuous-Thumb-Pinch-and-Sliding-Tracking)

## What could this work

By adjusting the value of 6 DOF, the simulated captured image will be provided and the 2D coordinate of thumb related to the index finger and index finger coordinate related to the thumb.

It could also save simulated images with the parameters as a data set. The data set includes:

* Values of 6 DOF
* Thumb coordinate
* Index finger coordinate
* Name of captured image
* PNG images (resolution: 640 * 480)

## Requirements

* Unity 2019.3.1f1
* Windows 10

## Scripts Analysis

| Name | Function |
| --- | --- |
| Common | Defines a common namespace, including folder actions and some common functions and enum classes |
| DatasetManager | Handles the input of parameters of dataset |
| DrawBoard | Draw the dot representing the coordinate of thumb |
| FolderIndicatorActions | Show the current folder path string |
| JointManger | Control the rotation of finger joints |
| ScreenShotManger | Get the parameters from DatasetManager and generate the dataset |
| Singleton | Single instance class template |
| SliderActions | Handle the user input and set the value to the JointManger |
| ToggleActions | Get the IsTouching and IsOverlapped flags from thumb |
| TouchDetection | Attached to the thumb and index finger collider, detect IsTouching, IsOverlapping, calculate the position of touch point, and calculate the 2D touch position of touch point. |
