using Overstrike.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Overstrike.Views;

/// <summary>
/// Floating popup window for displaying individual damage events
/// </summary>
public partial class DamagePopup : Window
{
    private readonly DamageEvent _damageEvent;
    private readonly Placement _placement;

    public DamagePopup(DamageEvent damageEvent, Placement placement)
    {
        InitializeComponent();

        _damageEvent = damageEvent;
        _placement = placement;

        ConfigureWindow();
        SetupContent();
        StartAnimation();
    }

    private void ConfigureWindow()
    {
        // Configure window properties for overlay
        Topmost = true;
        ShowInTaskbar = false;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;

        // Enhanced debug output
        Console.WriteLine($"Configuring popup window for {_damageEvent.Amount} damage");
        Console.WriteLine($"Placement rect: {_placement.WindowRect}, IsVisible: {_placement.IsVisible}");
        
        // Set position based on placement configuration
        Left = _placement.WindowRect.X + _placement.LastSpawnX;
        Top = _placement.WindowRect.Y + _placement.LastSpawnY;
        
        Console.WriteLine($"Window position set to: {Left}, {Top}");

        // Update spawn position for next popup
        _placement.LastSpawnX += 50;
        _placement.LastSpawnY += 30;
        
        // Set visibility explicitly
        Visibility = Visibility.Visible;

        // Reset position if it goes off screen
        if (Left > SystemParameters.PrimaryScreenWidth - 200)
        {
            _placement.LastSpawnX = 0;
        }
        if (Top > SystemParameters.PrimaryScreenHeight - 100)
        {
            _placement.LastSpawnY = 0;
        }
    }

    private void SetupContent()
    {
        // Set damage text
        DamageText.Text = _damageEvent.Amount > 0 ?
            _damageEvent.Amount.ToString("N0") :
            "MISS";

        // Set color based on damage type and critical hit
        var color = GetDamageColor();
        DamageText.Foreground = new SolidColorBrush(color);

        // Set font size based on damage amount or critical hit
        var fontSize = _damageEvent.IsCritical ? 24 : 18;
        if (_damageEvent.Amount > 1000)
        {
            fontSize += 4;
        }
        DamageText.FontSize = fontSize;

        // Add critical hit effects
        if (_damageEvent.IsCritical)
        {
            DamageText.FontWeight = FontWeights.Bold;
            DamageText.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 3,
                ShadowDepth = 2
            };
        }
    }

    private Color GetDamageColor()
    {
        return _damageEvent.Type switch
        {
            DamageType.Melee when _damageEvent.IsCritical => Colors.Red,
            DamageType.Melee => Colors.White,
            DamageType.Spell when _damageEvent.IsCritical => Colors.Magenta,
            DamageType.Spell => Colors.LightBlue,
            DamageType.Heal when _damageEvent.IsCritical => Colors.LimeGreen,
            DamageType.Heal => Colors.Green,
            DamageType.Miss => Colors.Gray,
            _ => Colors.White
        };
    }

    private void StartAnimation()
    {
        // Create movement animation based on direction
        var moveDirection = _placement.Direction;
        double moveX = 0, moveY = 0;

        // Enhance movement distance for better visibility
        int distance = 150;
        
        switch (moveDirection)
        {
            case Direction.Up:
                moveY = -distance;
                break;
            case Direction.Down:
                moveY = distance;
                break;
            case Direction.Left:
                moveX = -distance;
                break;
            case Direction.Right:
                moveX = distance;
                break;
        }

        Console.WriteLine($"Starting animation with direction {moveDirection}: moveX={moveX}, moveY={moveY}");

        // Create animations with slightly longer duration for testing
        var duration = TimeSpan.FromSeconds(3);

        // Movement animation
        var moveXAnimation = new DoubleAnimation(Left, Left + moveX, duration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var moveYAnimation = new DoubleAnimation(Top, Top + moveY, duration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // Fade out animation - start fading later for better visibility during testing
        var fadeAnimation = new DoubleAnimation(1.0, 0.0, duration)
        {
            BeginTime = TimeSpan.FromSeconds(1.5),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        // Scale animation for critical hits
        if (_damageEvent.IsCritical)
        {
            var scaleTransform = new ScaleTransform(1.0, 1.0);
            DamageText.RenderTransform = scaleTransform;
            DamageText.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new DoubleAnimation(1.0, 1.3, TimeSpan.FromSeconds(0.3))
            {
                AutoReverse = true,
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        // Start animations
        BeginAnimation(LeftProperty, moveXAnimation);
        BeginAnimation(TopProperty, moveYAnimation);
        BeginAnimation(OpacityProperty, fadeAnimation);

        // Close window when animation completes
        fadeAnimation.Completed += (s, e) => Close();
    }
}
