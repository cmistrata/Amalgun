using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTracker : MonoBehaviour
{
    public Team Team = Team.Enemy;
    public event Action<Team> ChangeTeamEvent;

    private Sprite _enemySprite;
    private Sprite _playerSprite;
    private SpriteRenderer _spriteRenderer;
    private EnemyController _enemyController;
    private MovingBody _movingBody;


    // Start is called before the first frame update
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _enemyController = GetComponent<EnemyController>();
        _movingBody = GetComponent<MovingBody>();
    }

    private void Start() {
        (_enemySprite, _playerSprite) = PrefabsManager.Instance.GetRandomEnemyAndPlayerBase();
    }

    public void ChangeTeam(Team newTeam) {
        Team = newTeam;

        UpdateSprites();
        if (_enemyController != null) _enemyController.enabled = Team == Team.Enemy;
        if (_movingBody != null && Team == Team.Neutral) _movingBody.StopMoving();
        gameObject.layer = 
            Team == Team.Player ? Layers.PlayerPart
            : Team == Team.Enemy ? Layers.EnemyPart
            : Layers.NeutralPart;
        if (ChangeTeamEvent != null) ChangeTeamEvent(newTeam);
    }

    void UpdateSprites() {
        if (Team == Team.Enemy) {
            _spriteRenderer.sprite = _enemySprite;
        } else {
            _spriteRenderer.sprite = _playerSprite;
        }
    }
}
