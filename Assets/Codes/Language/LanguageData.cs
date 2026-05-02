using UnityEngine;

[System.Serializable]
public class LanguageData
{
    public string play_button;
    public string settings_button;
    public string exit_button;
    public string level_button;
    public string resume_button;
    public string back_button;
    public string coming_soon;
    public string reset_level;
    public string reset_title;
    public string reset_desc;
    public string yes_button;
    public string no_button;
    public string hint_text;
    public string warning_panel;
    public string music_vol;
    public string sfx_vol;
    public string jump;
    public string double_jump;
    public string left;
    public string right;
    public string air_warning;
    public string ads_panel;
    public string total_death;
    public string total_time;
     public string[] page_titles;

 
   
    public string[] level_names_1;
    public string[] hints_1;
    public string[] extra_hints_1;

   
    public string[] level_names_2;
    public string[] hints_2;
    public string[] extra_hints_2;

 
    public string[] level_names_3;
    public string[] hints_3;
    public string[] extra_hints_3;

    
    public string[] level_names_4;
    public string[] hints_4;
    public string[] extra_hints_4;

 
    public string[] level_names_5;
    public string[] hints_5;
    public string[] extra_hints_5;

    [Header("Single Gyro Fallback (Sensör Yoksa Gelecek Metinler)")]
    public string gyro_level_name;   
    public string gyro_hint;         
    public string gyro_extra_hint;

    public string credit;
}
