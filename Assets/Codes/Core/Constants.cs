public static class Constants
{
    // --- PlayerPrefs Keys ---
    public const string PREF_TOTAL_DEATHS = "TotalDeaths";
    public const string PREF_TOTAL_TIME = "TotalTime";
    public const string PREF_MUSIC_VOLUME = "MusicVolume";
    public const string PREF_SFX_VOLUME = "SFXVolume";
    public const string PREF_SELECTED_LANG = "SelectedLang";
    public const string PREF_SELECTED_INTERNAL_INDEX = "SelectedInternalIndex";
    public const string PREF_LEVEL_UNLOCKED_PREFIX = "LevelUnlocked_";
    public const string PREF_LEVEL_COMPLETE_PREFIX = "LevelComplete_";
    public const string PREF_LAST_LEVEL_ID = "LastLevelID";

    // --- Tags ---
    public const string TAG_PLAYER = "Player";
    public const string TAG_OBSTACLE = "Obstacle";
    public const string TAG_GROUND = "Ground";
    public const string TAG_BOX = "Box";
    public const string TAG_BOX_BUTTON = "BoxButton";

    // --- Scenes ---
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_LEVELS = "Levels";
    public const string SCENE_MAP_SUFFIX = "Map"; // mapNum + Constants.SCENE_MAP_SUFFIX
}
