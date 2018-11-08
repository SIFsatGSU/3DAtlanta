
## 3D Atlanta

This ongoing interdisciplinary project has been in development since 2014 and has become one of the largest endeavors for the SIF team. Open World Atlanta recreates the blocks of 1928 Atlanta that surround present-day Georgia State buildings. Beginning with Decatur Street at Central Avenue (where Classroom South is now), the project has expanded to include the Candler and Flat Iron buildings to the north with future builds extending into historic Five Points.

Visit the Student Innovation Fellowship page about [Open World Atlanta](http://studentinnovation.gsucreate.org/projects/open-world-atlanta/) for more details.

***

## Table of Contents

- [Motivation](#motivation)
- [Build Status](#build-status)
- [Screenshots and Media](#screenshots-and-media)
- [Tools Used](#tech-and-frameworks-used)
- [Notable Features](#notable-features)
- [Code Examples](#code-example)
- [Current WorkFlow](#current-basic-workflow-outline)
- [Installation, Version, and Dependencies](#installation,-version,-dependencies)
- [How to use? (Deployment)](#how-to-use?-(deployment))
- [Current Goals](#current-goals)
- [Credits](#credits)

## Motivation
The goal for 3D Atlanta is to showcase an interactive space where students can experience the past as though they were ‘walking through’ it. Additionally, a Chronolens feature allows students to peer into the modern world at certain locations. In order to recreate the space, our team employs archived photographic material, maps, and modern digital scans to render each element. In addition, Open World Atlanta gives educators the opportunity to incorporate narratives taken from real events and artifacts so that students can interact with them in a game-like environment.


## Screenshots and Media
- The 3D Atlanta Demo Video

[![3D Atlanta Demo Video](https://img.youtube.com/vi/nQfp5wUFdZM/0.jpg)](https://www.youtube.com/watch?v=nQfp5wUFdZM)
- Bailey's Theatre
![Bailey's Theatre](https://i.imgur.com/RiPOtRp.jpg)


## Tech and Frameworks Used

<b>Built with</b>
- [Unity](https://unity3d.com/)
- [Visual Studio](https://visualstudio.microsoft.com/vs/) is the preferred coding environment. Usually is automatically installed with Unity. Unity's main scripting language is C# although there are a few other options such as a few different choices for visual programming. Look [here](https://docs.unity3d.com/Manual/VisualStudioIntegration.html) for more documentation on using C# with Visual Studio.

<b>Modeled with</b>
- [Blender](https://www.blender.org/)

## Notable Features
- The Chronolens and the Georgia State Comparison
    - The Chronolens is the feature that allows the user to look and compare the differences between Atlanta today and the representation of Atlanta circa 1928. At specific designated areas highlighted by the neon particle effects, the player can access the Chronolens.
    
[![Artifacts Demo Video](https://img.youtube.com/vi/AdSGrlQ73vo/0.jpg)](https://www.youtube.com/watch?v=AdSGrlQ73vo)

- Historic Landmark Buildings from Atlanta
    - Many landmark buildings that no longer exist are also in 3D Atlanta. Using photographs and any documentation that was recovered, they were all carefully modeled to look similar.

![Bailey's Theatre](https://i.imgur.com/sZtWspp.jpg)
![Peachtree Center Ave.](https://i.imgur.com/KYfetMy.jpg)

- Artifacts
    - Although this feature is a very subtle one of 3D Atlanta, there are artifacts placed all around the scene. These are all grabbable and interactable. (Make sure to look for all of them!)

[![Artifacts Demo Video](https://img.youtube.com/vi/u-GT4TWeGB8/0.jpg)](https://www.youtube.com/watch?v=u-GT4TWeGB8)

- Oculus Rift Support
    - You can explore the streets of Atlanta with your computer or even in VR! 3D Atlanta fully supports Oculus Rift VR headsets.

## Code Example
- Code Snippet From the Chronolens on Entering a Designated Future Area
~~~
public void AreaEnter(ChronolensArea area) {
		deactivateAudio.Stop (); //Stop the closing audio if not already.
		chronolenseMaterial.SetTexture ("_CubeMap", area.hdri);
		chronolenseMaterial.SetFloat ("_YawOffset", area.yawOffset);
		VibrateHand (initiateHapticsClip);

		currentState = "Initiate";
		currentStateLayer = 0;
		nextState = "Alpha 1";
		nextStateLayer = 1;
		PlayCurrentState ();
		Audios.PlayAudio (initiateAudio);
	}
~~~

## Current Basic WorkFlow Outline

**Database>Packages>Unity and CityEngine**

Currently a lot of effort is being put into the database of 3D buildings for the packages of the project. These packages will help facilitate the outsourcing of buildings' history, models, and any other relevant information to students and teachers.

The Unity side will be mainly experimenting with the scale, placement, and interaction of the buildings. The CityEngine side will focus on using the data from the ArcGIS suite in creating a city in CityEngine.


 Read more about the project workflow in the [Wiki](https://github.com/SIFsatGSU/3DAtlanta/wiki).


## Installation, Version, and Dependencies
- The github repository can simply be cloned to your personal computer and can be run with Unity.
- The Unity version is specifically **2017.3.0b8** with currently no plans of upgrading.
- Any version of Blender **must** be installed on your device for many models to be displayed in your Unity scene. This is currently a known issue that will be fixed in the future. Some models were imported into Unity as the native Blender file. Instead, these should have been exported as .fbx files. This is the cause for the issue of missing prefabs.

## How to use? (Deployment)
- This project currently does not have a specific location where a stable build can be downloaded. Instead, you can git clone this repository and run it from Unity directly.

1. Git clone this repository into a local directory.
    * Alternativley and more easily, you could download the .zip folder and unpack it into the directory.
2. Install Unity and Blender.
    * Look at [Tech and Frameworks Used](#tech-and-frameworks-used)
3. Execute Unity and open the project folder that you have retrieved from Github.
4. Remember to use Unity Version **2017.3.0b8**!
5. Enjoy exploring the streets of 1928 Atlanta.

## Current Goals
- [ ] Adding more cubemap locations for the chronolens
- [ ] Making artifacts more interactable by including information snippets when found
- [ ] Add more detailed UI for ease of use for new players
- [X] Transfer models to new upcoming AR Atlanta Project!

## Credits
- Major Past Contributor
    * Jack Le
        * [Github](https://github.com/jackle1127)
    * Wasfi Momen
        * [Github](https://github.com/CodeFluent)

-Student Fellow Contributors for the 2018-2019 year

| Name          | Major           | SIF Page                                                                         |   
| ------------- |:-------------:  | :-------------------------------------------------------------------------------:|
| Lee Klarich   | Computer Science| [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/lee-klarich/)     |
| Blaire Bosley | History         | [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/blaire-bosley/)   |
| Sydney Adams  | Computer Science| [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/sydney-adams/)    |
| Saif Ali      | Astrophysics    | [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/saif-ali/)        |
| Shaynah Ruff  | Game Design     | [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/shaynah-ruff/)    |
| Chris Kim     | Computer Science| [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/chris-kim/)       |
| Joel Mack     | Game Design     | [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/joel-austin-mack/)|

-Project Management

| Name            | Role               | SIF Page                                                                        |   
| ----------------|:------------------:| :------------------------------------------------------------------------------:|
| Brennan Collins | Program Co-Director| [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/brennan-collins/)|
| Spencer Roberts | Program Co-Director| [Link](http://studentinnovation.gsucreate.org/meet-the-fellows/spencer-roberts/)|

## Etc.
- Specific photos with watermark are property of Georgia State Library