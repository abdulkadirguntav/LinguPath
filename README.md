# 🦊 LinguPath

**AI-Powered Mobile English Learning Game**

LinguPath is a Serious Game project that combines gamification and large language model (LLM) technology. Players explore a low-poly town environment, interact with NPC characters, and learn English through a variety of mini-games.

> **Nevşehir Hacı Bektaş Veli University** — Computer Engineering Undergraduate Capstone Project (2026)  
> **Developer:** Abdülkadir Güntav

---

## 🎥 Gameplay Showcase



https://github.com/user-attachments/assets/bafaf2b6-1268-4d0a-afad-743101671720



---

## 🕹️ Game Mechanics

Each NPC triggers a mini-game. Completing a game marks that NPC as permanently finished.

### 🃏 Swipe Game (School)
A visual and sentence pair appears on screen. Swipe right if the sentence matches the visual, left if it doesn't. Each round shows 7 cards; you have 3 lives.  
*Data source:* `Resources/SwipeCards.csv`

### 📚 Word Learning (Library)
10 words are shown per session. The English word and an example sentence appear first; tap "Show" to reveal the Turkish translation. `KNOW` increases mastery (0–3), `STUDY` sends the word back to the deck. Each visit advances through pages of 10 words in order.  
*Data source:* `Resources/Words/` (ScriptableObject)

### 🛒 Market Game (Supermarket)
The NPC gives you an English shopping list. Tap items in the scene to add them to your cart, then hit "Checkout" to verify. The list is compared regardless of selection order.

### ⚽ Football Duel (Turn-Based Word Game)
A turn-based game with two phases:
- **Attack (15s):** Arrange shuffled words in the correct order to form the sentence from the Turkish clue — score a goal.
- **Defense (5s):** Pick the odd word out from 4 options; wrong answer = concede a goal.

First to 5 goals wins.  
*Data sources:* `Resources/Sentences.csv`, `Resources/Defense.csv`

### 🤖 AI Fox Chat (Town Square)
Practice free English conversation with the fox mascot Foxy. Each response includes grammar feedback. Both text and voice (microphone) input are supported; audio is recorded as WAV and sent to Gemini for transcription. Oculus LipSync SDK handles lip synchronization.  
*Model:* `gemini-2.5-flash` — API key is read from `StreamingAssets/api_config.json`.

---

## 🏗️ Technical Architecture

```text
Assets/
├── Scripts/
│   ├── AI/                  # GeminiManager, MicrophoneInputManager
│   ├── Camera/              # CameraFollow
│   ├── Character/           # Character creator and save system
│   ├── Core/                # CoreSettingsManager
│   ├── CSV/                 # CSV reader (market, swipe)
│   ├── FOX/                 # FoxBlink animation
│   ├── Game/
│   │   ├── SwipeGame/       # SwipeManager, SwipeCard
│   │   ├── MarketGame/      # MarketManager, CartItemUI, MarketItemButton
│   │   └── TurnBaseGame/    # TurnBaseManager
│   ├── Library/             # LibraryManager (word learning)
│   ├── Main Menu/           # MainMenuManager, AudioManager, SettingsManager
│   ├── NPC's/               # NPC dialogue and quest system
│   ├── Player/              # PlayerMovement (joystick support)
│   ├── PlayerPref/          # DataManager, WordProgress
│   ├── SaveSystem/          # 3-slot JSON save system
│   └── Scriptable Object/   # CardDataSO, NPCDialogueSO, WordDataSO
├── Scene/
│   ├── MainMenu.unity
│   └── Core.unity           # Town map + all mini-games
└── Resources/
    ├── SwipeCards.csv
    ├── Sentences.csv
    ├── Defense.csv
    ├── Icons/               # Market product visuals
    ├── SwipeImage/          # Swipe card visuals
    └── Words/               # WordDataSO assets
```

### Save System
3 independent game slots are written as JSON to `Application.persistentDataPath/slot_N.json`. Each slot stores character data, word mastery levels (0–3), and NPC quest states.

### NPC System
Each NPC exists in one of three states: `BeforeMission` / `MissionFailed` / `MissionCompleted`. When the player approaches, dialogue begins via `NPCDialogueSO`; once dialogue ends, the mini-game launches. Mission results are written to disk immediately.

---

## Technologies Used

| Technology | Purpose |
|---|---|
| Unity 6000.0.67f1 LTS | Game engine |
| C# | Game logic, UI, NPC system |
| Google Gemini 2.5 Flash API | AI chat + speech transcription |
| Oculus LipSync SDK | Foxy lip synchronization |
| Newtonsoft.Json | API response parsing |
| TextMeshPro | UI text rendering |
| SimplePoly City | Low-poly town assets |
| Joystick Pack | Mobile movement controls |

---

## Setup

### Requirements
- Unity 6000.0.67f1 LTS + Android Build Support module
- Google Gemini API key (with `gemini-2.5-flash` access)

### Steps

1. Clone the repo and open the project in Unity Hub.

2. Create `api_config.json` inside `Assets/StreamingAssets/`:
   ```json
   {
     "geminiApiKey": "YOUR_API_KEY_HERE"
   }
   ```

3. `File > Build Settings` → Android → `Build`.

4. Install the APK on an Android 5.1+ (API 22+) device.

> Microphone permission is required for voice input; the app will request it automatically on first use.

---

## Data Formats

| File | Format |
|---|---|
| `SwipeCards.csv` | `id;imageName;correctSentence;wrongSentence` |
| `Sentences.csv` | `id;word1 word2 word3;trap1,trap2;Turkish Clue` |
| `Defense.csv` | `id;oddWord;normal1,normal2,normal3` |
| `WordDataSO` | ScriptableObject: `wordID`, `englishWord`, `turkishMeaning`, `exampleSentences` |

---

## Roadmap

- [ ] Word retention analytics and statistics screen
- [ ] Multi-language support (German, French)
- [ ] Online multiplayer Football Duel mode
- [ ] iOS support

---

## References

- Prensky, M. (2001). *Digital Game-Based Learning*. McGraw-Hill.
- Deterding, S. et al. (2011). From game design elements to gamefulness: Defining gamification. *ACM MindTrek Conference*.
- [Unity Documentation](https://docs.unity3d.com)
- [Google Gemini API Docs](https://ai.google.dev/docs)
- [Oculus LipSync SDK](https://developer.oculus.com/documentation/unity/audio-ovrlipsync-unity)

---

## License

This project was developed as an undergraduate capstone project.  
© 2026 Abdülkadir Güntav

al düzenle video'yu nereye atacam
