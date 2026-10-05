using UnityEngine;

namespace DPSMeter;

public sealed class DPSMeter : ModBehaviour
{
    public static DPSMeter Instance { get; private set; }

    private ClientEventManager _clientEvents;
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

    private void OnTakeDamage(EventInfoDamage info)
    {
        if (info.actor == null || info.victim == null)
        {
            return;
        }

        DewPlayer local = DewPlayer.local;
        if (local == null || local.hero == null)
        {
            return;
        }

        if (info.actor.firstEntity != local.hero)
        {
            return;
        }

        float amount = info.damage.amount;
        if (amount <= 0f)
        {
            return;
        }

        SkillTrigger skill = info.actor.firstTrigger as SkillTrigger;
        AbilityInstance ability = info.actor.FindFirstOfType<AbilityInstance>();
        Gem gem = ability != null ? ability.gem : info.actor.FindFirstOfType<Gem>();

        string skillName = skill != null ? skill.GetFormattedSkillTitle() : "Basic / Other";
        string gemName = gem != null ? gem.GetActorReadableName() : "No Essence";

        _data.AddDamage(amount, skillName, gemName);
    }

    [ModBehaviour.ConsoleCommand("Reset the DPS meter.", "dps_reset")]
    private void ResetMeter()
    {
        _data.Reset();
    }

    [ModBehaviour.ConsoleCommand("Toggle the DPS meter overlay.", "dps_meter")]
    private void ToggleMeter()
    {
        if (_overlay != null)
        {
            _overlay.Visible = !_overlay.Visible;
        }
    }

    private void OnDestroy()
    {
        DetachFromClientEvents();

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
