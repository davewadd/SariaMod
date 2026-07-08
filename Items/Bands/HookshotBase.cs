using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.Buffs;

namespace SariaMod.Items.Bands
{
/// <summary>
/// Shared configuration and constants for Hookshot and Longshot
/// </summary>
public static class HookshotConfig
{
    // ===== DEBUG OPTIONS =====
    /// <summary>Set to true to show a red rectangle during the sweetspot window</summary>
    public const bool ShowSweetspotDebugRectangle = false;
    
    // ===== SHARED TIMING CONSTANTS =====
    /// <summary>Total frames before auto-pull triggers (or early release if debuffed)</summary>
    public const int EnemyHoldTime = 200;
    /// <summary>Sweetspot window start frame (170-188 = sweetspot)</summary>
    public const int SweetspotWindowStart = 160;
    /// <summary>Sweetspot window end frame (inclusive)</summary>
    public const int SweetspotWindowEnd = 184;
    /// <summary>Frame to play audio/visual cue for sweetspot timing</summary>
    public const int SweetspotCueFrame = 160;
    public const float CombatPullSpeed = 48f;
        
    /// <summary>Duration of dash cooldown debuff in frames (5 seconds = 300 frames)</summary>
    public const int DashCooldownDuration = 300;
    
    /// <summary>Maximum time for attack pull before auto-disconnect (8 seconds = 480 frames)</summary>
    public const int PullTimeoutFrames = 480;
        
    // ===== SHARED DISTANCES =====
    public const float ArrivalDistance = 48f;
    public const float MinPullDistanceForSuccess = 48f;
    public const float HookDetectionRadius = 24f;
    public const float HookAttachDistance = 32f;
}
}
