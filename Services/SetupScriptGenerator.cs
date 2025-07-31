namespace HighlightReel.Services
{
    public static class SetupScriptGenerator
    {
        public static void Generate(string cs2Path)
        {
            var baseDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cs2Path), "..", ".."));
            var cfgDir = Path.Combine(baseDir, "csgo", "cfg");
            Directory.CreateDirectory(cfgDir);

            var path = Path.Combine(cfgDir, "setup_script.cfg");
            File.WriteAllText(path, string.Join(Environment.NewLine, new[]
            {
                "sv_cheats 1",
                "echo \"--- Applying cinematic settings... ---\"",
                "cl_draw_only_deathnotices 1; mp_display_kill_assists 0; spec_show_xray 0; net_graph 0;",
                "cl_viewmodel_shift_left_amt 0; cl_viewmodel_shift_right_amt 0;",
                "cl_bob_lower_amt 5; cl_bobamt_lat 0; cl_bobamt_vert 0; cl_bobcycle 2;",
                "viewmodel_offset_x 2; viewmodel_offset_y 0; viewmodel_offset_z -2;",
                "voice_enable 0; bot_chatter off; snd_setmixer dialog vol 0; sv_ignoregrenaderadio 1;"
            }));
        }
    }
}
