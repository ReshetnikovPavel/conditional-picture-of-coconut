using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using Game;
using GridTools;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Enemies
{
    public class PurpleWizard : MonoBehaviour
    {
        // Start is called before the first frame update
        public Weapon.Weapon Weapon;

        private float latestAimAngle;

        private Vector3 directionFire;

        private float fireRange = 10f;

        public HealthObj Health;
        [SerializeField] private int maxHealth;
        private Rigidbody2D Rb;
        private Collider2D Collider;
        private SpriteRenderer sprite;
        public GameObject healthObjPrefab;

        private Stage currentStage;
        private State state;

        public GridObj Grid;
        private PathFinding pathFinder;

        private Vector3 homePosition;
        private float homeRadius;

        private Vector3 startingPosition;
        private Vector3 roamPosition;
        private Vector3 nextTarget;
        private int nextTargetIndex;

        private int countFailSearch;
        private const int countFailSearchLimit = 5;

        private Vector3 direction;

        private List<int2> path;

        private const float pauseTime = 1f;
        private const float followingTime = 6f;
        private const float reloadTime = 1f;
        private float reloadStart;
        private float pauseStart;
        private float followingStartTime;

        private float targetRange = 25f;
        private float RunRange = 7f;
        private float moveSpeed;

        private enum Stage
        {
            None,
            SearchingPath,
            Moving,
            Pause
        }

        private enum State
        {
            Roaming,
            RunToPlayer,
            RunFromPlayer
        }

        private void Start()
        {
            pathFinder = new PathFinding();
            Health = Instantiate(healthObjPrefab, transform).GetComponent<HealthObj>();
            Health.maxHealthPoints = maxHealth;
            sprite = GetComponent<SpriteRenderer>();
            Rb = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();

            homePosition = transform.position;
            startingPosition = transform.position;
            UpdateTarget(GetRandomPosition());

            homeRadius = 25;

            currentStage = Stage.None;
            moveSpeed = 3f;
            followingStartTime = Time.time;
            reloadStart = Time.time;
        }

        private void FixedUpdate()
        {
            if (Health.CurrentHealthPoints <= 0)
                Die();

            if (IsNearToPlayer(targetRange))
                UpdateAimFire(GameData.player.transform.position);
            else
                UpdateAimFire(nextTarget);

            if (IsNearToPlayer(fireRange))
                Fire();

            ChooseBehaviour();

            switch (state)
            {
                case State.Roaming:
                    if (countFailSearch > 0)
                        UpdateTarget(countFailSearch >= countFailSearchLimit
                            ? homePosition
                            : GetRandomPosition());
                    UpdateEyeDirection(nextTarget);
                    Move(roamPosition);
                    break;
                case State.RunFromPlayer:
                    var playerPosition = GameData.player.GetPosition();
                    do roamPosition = GetRandomPosition();
                    while (roamPosition.DistanceTo(playerPosition) < RunRange);

                    UpdateTarget(roamPosition);
                    UpdateEyeDirection(nextTarget);
                    MoveWithTimer(roamPosition, followingTime);
                    break;
                case State.RunToPlayer:
                    UpdateTarget(GameData.player.GetPosition());
                    UpdateEyeDirection(GameData.player.GetPosition());
                    MoveWithTimer(roamPosition, followingTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Move(Vector3 target)
        {
            switch (currentStage)
            {
                case Stage.None:
                    currentStage = Stage.SearchingPath;
                    StartSearchNextTarget(target);
                    break;
                case Stage.SearchingPath:
                    break;
                case Stage.Moving:
                    MoveToNextTarget();
                    break;
                case Stage.Pause:
                    var difference = Time.time - pauseStart;
                    if (difference >= pauseTime)
                    {
                        UpdateTarget(GetRandomPosition());
                        currentStage = Stage.None;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveWithTimer(Vector3 target, float timeFollow)
        {
            var timeFollowing = Time.time - followingStartTime;
            if (timeFollowing >= timeFollow && currentStage != Stage.SearchingPath)
            {
                currentStage = Stage.None;
                followingStartTime = Time.time;
                return;
            }

            Move(target);
        }

        private Task<List<int2>> FindPath(int2 startGridPosition, int2 endGridPosition, int maxDeep)
        {
            var task = new Task<List<int2>>(() =>
                pathFinder.FindPathAStar(Grid.Grid, startGridPosition, endGridPosition, maxDeep));

            task.Start();
            return task;
        }

        private async void StartSearchNextTarget(Vector3 target)
        {
            UpdateTarget(target);

            var startGridPosition = Grid.WorldToGridPosition(startingPosition);
            var endGridPosition = Grid.WorldToGridPosition(roamPosition);

            var maxDeep = (int)homeRadius;
            var originalPath = await FindPath(startGridPosition, endGridPosition, maxDeep);

            if (originalPath is null)
            {
                currentStage = Stage.None;
                countFailSearch++;
            }
            else
            {
                countFailSearch = 0;
                path = PathFinding.GetClearPath(originalPath);
                Grid.AddPathsToDraw(path);

                nextTargetIndex = 0;
                UpdateNextTarget();
            }
        }

        private void MoveToNextTarget()
        {
            UpdateDirection(nextTarget);
            var distanceToNextTarget = transform.position.DistanceTo(nextTarget);

            Rb.velocity = direction * moveSpeed;

            if (distanceToNextTarget >= moveSpeed * Time.fixedDeltaTime)
                return;

            if (nextTargetIndex == path.Count - 1)
            {
                Rb.velocity = Vector2.zero;
                currentStage = Stage.Pause;
                pauseStart = Time.time;
                return;
            }

            UpdateNextTarget();
        }

        private void UpdateNextTarget()
        {
            for (var i = path.Count - 1; i > nextTargetIndex; i--)
            {
                var target = Grid.GridToWorldPosition(path[i]).ToVector3() + new Vector3(Grid.Grid.CellSize, Grid.Grid.CellSize) / 2;
                var currentPosition = transform.position;
                var distance = currentPosition.DistanceTo(target);
                var currentDirection = (target - currentPosition).normalized;

                var ray = Physics2D.CircleCast(currentPosition.ToVector2(), transform.localScale.y, currentDirection.ToVector2(), distance, Grid.WallsLayerMask);

                if (ray.collider != null)
                    continue;

                nextTargetIndex = i;
                nextTarget = target;
                currentStage = Stage.Moving;
                return;
            }

            nextTargetIndex++;
            nextTarget = Grid.GridToWorldPosition(path[nextTargetIndex]).ToVector3() + new Vector3(Grid.Grid.CellSize, Grid.Grid.CellSize) / 2;
            currentStage = Stage.Moving;
        }

        private void UpdateTarget(Vector3 target)
        {
            startingPosition = transform.position;
            roamPosition = homePosition.DistanceTo(target) > homeRadius
                ? homePosition
                : target;
        }

        private void UpdateDirection(Vector3 target)
        {
            direction = (nextTarget - transform.position).normalized;
        }

        private Vector3 GetRandomPosition()
            => homePosition + Tools.GetRandomDir() * Random.Range(10f, 15f);

        private void UpdateEyeDirection(Vector3 target)
        {
            sprite.flipX = (int)Mathf.Sign(target.x - transform.position.x) == 1;
        }

        private void Fire()
        {
            var difference = Time.time - reloadStart;
            if (difference < reloadTime)
                return;
            Weapon.Fire(true);
            reloadStart = Time.time;
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        private bool IsNearToPlayer(float distance)
        {
            try
            {
                return Vector3.Distance(transform.position, GameData.player.transform.position) < distance;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateAimFire(Vector3 target)
        {
            directionFire = (target - transform.position).normalized;
            var aimAngle = Mathf.Atan2(directionFire.y, directionFire.x) * Mathf.Rad2Deg - 90f;
            Weapon.weaponPrefab.transform.RotateAround(Rb.position, Vector3.forward, aimAngle - latestAimAngle);
            latestAimAngle = aimAngle;
        }

        private void ChooseBehaviour()
        {
            if (homePosition.DistanceTo(transform.position) > homeRadius)
                UpdateTarget(homePosition);

            if (IsNearToPlayer(targetRange))
            {
                if (IsNearToPlayer(RunRange))
                {
                    if (state != State.RunFromPlayer)
                        followingStartTime = int.MinValue;
                    state = State.RunFromPlayer;
                }
                else
                    state = State.RunToPlayer;
            }

            else
                state = State.Roaming;
        }
    }
}
