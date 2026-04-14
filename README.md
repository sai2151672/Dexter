# Dexter – Your Keyboard-Away Assistant

## Overview

Dexter is a lightweight, voice-controlled desktop assistant for Windows that enables users to control their computer without relying on a keyboard. Built using C#, WPF, and System.Speech, Dexter is designed to be fast, customizable, and practical for everyday use. It allows users to interact with their system through natural voice commands such as opening applications, closing programs, and managing multiple tasks at once.

Commands like “open chrome and notepad” or “close discord” demonstrate its ability to handle multiple actions in a single request, while wake and sleep phrases such as “hello dexter” and “bye dexter” create a seamless hands-free experience.

---

## Purpose

The core idea behind Dexter is to provide an all-in-one assistant that removes friction between the user and their computer. Instead of switching between keyboard, mouse, and applications, Dexter allows users to stay focused and productive by interacting through voice alone.

Whether working on multiple tasks, presenting, coding, or simply aiming to improve efficiency, Dexter acts as a central control layer that simplifies how users interact with their system. Its JSON-based configuration system allows for easy customization, enabling users to define their own applications, aliases, and behaviors without modifying the core codebase.

---

## Features

* Voice-controlled application launching and closing
* Multi-command support (e.g., “open chrome and calculator”)
* Wake and sleep functionality (“hello dexter”, “bye dexter”)
* JSON-based configuration for easy customization
* Alias support for flexible command recognition
* System tray integration
* Live configuration reload

---

## Developer

Dexter is developed and maintained by Sai Dasari. This project is a personal initiative focused on building practical, real-world software that improves everyday workflows through intuitive interaction.

---

## Support the Project

If you find Dexter useful or interesting, consider supporting its development. As an independent project built during personal time, any support helps contribute to further improvements, new features, and long-term sustainability.

You can support this project through platforms such as:

Ko-Fi: ko-fi.com/saidasari

---

## Coming Soon

Dexter is actively evolving, with several features planned for future releases:

* Improved natural language understanding
* Text-to-speech responses
* Web automation capabilities
* File and system-level control
* Plugin system for extensibility
* AI-powered command generation
* Activity tracking and usage insights

---

## Configuration

Dexter uses a simple JSON-based configuration system to define applications and their aliases:

```json
{
  "name": "chrome",
  "aliases": ["chrome", "google", "browser"],
  "launchTarget": "chrome.exe"
}
```

This makes it easy to extend Dexter’s capabilities without modifying the source code.

---

## Tech Stack

* C#
* WPF (.NET)
* System.Speech Recognition
* JSON Configuration

---

## Closing Note

Dexter represents a step toward more intuitive computing, where users interact with their systems naturally rather than through rigid input methods. It is both a functional tool and an ongoing exploration of how voice-driven interfaces can enhance productivity and accessibility.
