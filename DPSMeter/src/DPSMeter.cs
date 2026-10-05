using UnityEngine;

namespace DPSMeter;

public sealed class DPSMeter : ModBehaviour
{
    public static DPSMeter Instance { get; private set; }

    private ClientEventManager _clientEvents;
    private ZoneManager _zoneManager;
    private DpsData _data;
    private DpsOverlay _overlay;
    private bool _subscribed;

    private void Awake()
    {
        Instance = this;
        _data = new DpsData();
        _overlay = gameObject.AddComponent<DpsOverlay>();
        _overlay.Initialize(_data);

        CallOnNetworkedManager<ClientEventManager>(AttachToClientEvents, DetachFromClientEvents);
        CallOnNetworkedManager<ZoneManager>(AttachToZoneManager, DetachFromZoneManager);
    }

    private void AttachToClientEvents()
    {
        _clientEvents = ClientEventManager.instance;

        if (_clientEvents == null || _subscribed)
        {
            return;
        }

        _clientEvents.OnTakeDamage += OnTakeDamage;
        _subscribed = true;
        Debug.Log("[DPS Meter] Damage event listener attached.");
    }

    private void DetachFromClientEvents()
    {
        if (_clientEvents != null && _subscribed)
        {
            _clientEvents.OnTakeDamage -= OnTakeDamage;
        }

        _clientEvents = null;
        _subscribed = false;
    }

    private void AttachToZoneManager()
    {
        _zoneManager = ZoneManager.instance;

        if (_zoneManager == null)
        {
            return;
        }

        _zoneManager.ClientEvent_OnZoneLoadStarted += OnZoneLoadStarted;
        Debug.Log("[DPS Meter] Zone reset listener attached.");
    }

    private void DetachFromZoneManager()
    {
        if (_zoneManager != null)
        {
            _zoneManager.ClientEvent_OnZoneLoadStarted -= OnZoneLoadStarted;
        }

        _zoneManager = null;
    }

    private void OnZoneLoadStarted(EventInfoLoadZone info)
    {
        _data.ResetCurrentInstance();
    }

    private void OnTakeDamage(EventInfoDamage info)
    {
        if (info.actor == null || info.victim == null)
        {
            return;
        }

        float appliedDamage = Mathf.Max(0f, info.damage.amount);
        float producedDamage = appliedDamage + Mathf.Max(0f, info.damage.discardedAmount);

        if (producedDamage <= 0f)
        {
            return;
        }

        DewPlayer local = DewPlayer.local;

        if (local == null || local.hero == null)
        {
            return;
        }

        Hero sourceHero = info.actor.firstEntity as Hero;

        if (sourceHero == null)
        {
            return;
        }

        DewPlayer sourcePlayer = FindPlayer(sourceHero);

        if (sourcePlayer == null || !sourcePlayer.isHumanPlayer)
        {
            return;
        }

        bool isLocalPlayer = sourcePlayer == local;

        AbilityInstance ability = info.actor.FindFirstOfType<AbilityInstance>();
        Gem gem = ability != null ? ability.gem : null;
        SkillTrigger skill = info.actor.firstTrigger as SkillTrigger;

        string skillName = skill != null
            ? skill.GetFormattedSkillTitle()
            : null;

        string essenceName = gem != null
            ? GetEssenceLabel(gem)
            : null;

        string playerName = isLocalPlayer ? "You" : sourcePlayer.playerName;

        _data.AddDamage(
            producedDamage,
            appliedDamage,
            isLocalPlayer,
            skillName,
            essenceName,
            playerName);
    }

    private static string GetEssenceLabel(Gem gem)
    {
        if (gem == null)
        {
            return null;
        }

        string typeName = gem.GetType().Name;

        if (typeName.StartsWith("Gem_"))
        {
            string name = typeName.Substring(4);
            int separator = name.IndexOf('_');

            if (separator >= 0)
            {
                name = name.Substring(separator + 1);
            }

            return "Essence of " + SplitPascalCase(name);
        }

        return typeName;
    }

    private static string SplitPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Unknown";
        }

        System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length + 8);

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]))
            {
                result.Append(' ');
            }

            result.Append(c);
        }

        return result.ToString();
    }

    private DewPlayer FindPlayer(Hero hero)
    {
        if (hero == null || DewPlayer.gamePlayers == null)
        {
            return null;
        }

        for (int i = 0; i < DewPlayer.gamePlayers.Count; i++)
        {
            DewPlayer player = DewPlayer.gamePlayers[i];

            if (player != null && player.hero == hero)
            {
                return player;
            }
        }

        return null;
    }

    [ModBehaviour.ConsoleCommand("Toggle the DPS meter overlay.", "dps_meter")]
    private void ToggleMeter()
    {
        if (_overlay != null)
        {
            _overlay.Visible = !_overlay.Visible;
        }
    }

    [ModBehaviour.ConsoleCommand("Reset the DPS meter.", "dps_reset")]
    private void ResetMeter()
    {
        if (_data != null)
        {
            _data.Reset();
        }
    }

    private void OnDestroy()
    {
        DetachFromClientEvents();
        DetachFromZoneManager();

        if (_overlay != null)
        {
            Destroy(_overlay);
            _overlay = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}