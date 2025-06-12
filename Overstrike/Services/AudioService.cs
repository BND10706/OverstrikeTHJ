using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Overstrike.Models;
using System.IO;

namespace Overstrike.Services;

/// <summary>
/// Service for playing audio notifications using NAudio
/// </summary>
public class AudioService : IAudioService, IDisposable
{
    private readonly ILogger<AudioService> _logger;
    private IWavePlayer? _wavePlayer;
    private bool _isEnabled = true;
    private float _volume = 0.5f;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0f, 1.0f);
    }

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;
        InitializeAudioDevice();
    }

    private void InitializeAudioDevice()
    {
        try
        {
            _wavePlayer = new WaveOutEvent();
            _logger.LogInformation("Audio device initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize audio device");
        }
    }

    public async Task PlaySoundAsync(string soundPath)
    {
        if (!_isEnabled || _wavePlayer == null) return;

        if (!File.Exists(soundPath))
        {
            _logger.LogWarning("Sound file not found: {SoundPath}", soundPath);
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                using var audioFileReader = new AudioFileReader(soundPath);
                audioFileReader.Volume = _volume;

                _wavePlayer.Init(audioFileReader);
                _wavePlayer.Play();

                // Wait for playback to complete
                while (_wavePlayer.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play sound: {SoundPath}", soundPath);
        }
    }

    public async Task PlayNotificationAsync(DamageType damageType, bool isCritical)
    {
        if (!_isEnabled) return;

        // Determine sound file based on damage type and critical hit
        var soundFileName = damageType switch
        {
            DamageType.Melee when isCritical => "melee_crit.wav",
            DamageType.Melee => "melee_hit.wav",
            DamageType.Spell when isCritical => "spell_crit.wav",
            DamageType.Spell => "spell_hit.wav",
            DamageType.Heal when isCritical => "heal_crit.wav",
            DamageType.Heal => "heal.wav",
            DamageType.Miss => "miss.wav",
            _ => null
        };

        if (soundFileName == null) return;

        var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", soundFileName);

        if (File.Exists(soundPath))
        {
            await PlaySoundAsync(soundPath);
        }
        else
        {
            // Generate default beep sound for missing audio files
            await GenerateBeepSoundAsync(damageType, isCritical);
        }
    }

    private async Task GenerateBeepSoundAsync(DamageType damageType, bool isCritical)
    {
        try
        {
            await Task.Run(() =>
            {
                // Simply log the event instead of generating a beep
                // Console.Beep is not supported on all platforms
                _logger.LogInformation("Sound effect for {Type} {Critical}", 
                    damageType, isCritical ? "critical" : "normal");
                
                // No sound on macOS, just log it
                Console.WriteLine($"Sound played: {damageType} {(isCritical ? "critical" : "normal")}");
                
                // Small delay to simulate sound duration
                Thread.Sleep(isCritical ? 200 : 100);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate sound effect");
        }
    }

    public void Dispose()
    {
        _wavePlayer?.Dispose();
        _wavePlayer = null;
    }
}
