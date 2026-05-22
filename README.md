# Normal Game

## Project Overview
**Normal Game** is a mobile-first 2D troll platformer designed to test players' patience with unpredictable and deceptive obstacles. 

Originally developed as an academic course assignment, this project has been successfully published on the Google Play Store. It serves as a strong portfolio piece showcasing my skills in gameplay programming in Unity, mobile UI architecture, dynamic trap algorithms, and full mobile publishing pipelines (including AdMob and GDPR compliance). The visual and UI assets were created by a teammate, while the entire software architecture, core mechanics, and store integrations were solely developed by me.

## Technologies Used
* **Game Engine:** Unity 6
* **Programming Language:** C#
* **Monetization & Legal:** Google AdMob SDK, Google UMP (User Messaging Platform)
* **2D Graphics / UI:** Teammate

## Key Features
* **Dynamic Level Mechanics:** Incorporates custom puzzle mechanics alongside traditional platforming, such as the "Blind Memory" challenge, testing the player's perception and problem-solving skills.
* **Robust Level Selection System:** A highly functional, paginated UI utilizing `PlayerPrefs` to track player progression, automatically navigate to the correct level page, and efficiently manage locked/unlocked states.
* **Localization System:** A built-in JSON-based `LocalizationManager` for seamless multi-language support (e.g., English, Turkish), ensuring UI elements and in-game text are easily accessible to a global audience.
* **Hint System:** A resource management structure utilizing an "Extra Hint" system that provides optional guidance to players during difficult levels.
* **Mobile UI & Optimization:** Touch-friendly mobile controls and responsive UI systems optimized to run flawlessly across various Android screen resolutions and aspect ratios.
* **Optimized Architecture:** Utilizes Singleton-based managers (e.g., GameManager, LevelUIManager), Coroutine-based sequences for smooth level transitions, and object pooling to ensure a stutter-free, high-performance experience on mobile devices.
* **Integrated Monetization:** A clean implementation of `AdMobInterstitialManager` to serve interstitial ads during natural gameplay pauses without disrupting the core user experience.

## Screenshots & Gameplay
<img width="526" height="258" alt="unnamed" src="https://github.com/user-attachments/assets/159285f0-a10d-4f80-bb4f-fef3e82a740a" />
<img width="526" height="243" alt="unnamecd" src="https://github.com/user-attachments/assets/696d313b-cefb-4522-b1b8-e51b644f2ed2" />


## About the Developer
I am a Digital Game Design student at Kahramanmaraş İstiklal University. I primarily focus on systems programming, UI architecture, and game mechanics using C# within the Unity engine.

* **LinkedIn:** https://www.linkedin.com/in/abdullah-%C3%A7elik-35b06b205/
* **Google Play Store:** https://play.google.com/store/apps/details?id=com.istiklal.doga.y2026.normalgame

## License
Copyright (c) 2026 Abdullah Çelik

All Rights Reserved.

The content of this repository (source code, 2D/3D models, audio, animations, and all other assets) is shared publicly strictly for portfolio and demonstration purposes. 

No materials, code snippets, or assets within this project may be copied, modified, distributed, or utilized in any commercial or non-commercial projects without explicit permission.
