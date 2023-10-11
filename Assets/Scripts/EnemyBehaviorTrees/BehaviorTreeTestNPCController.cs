using System.Collections;
using EnemyBehaviorTrees.Actions;
using UnityEngine;
using UnityEngine.AI;
using WUG.BehaviorTreeVisualizer;
using EnemyBehaviorTrees.Composites;
using EnemyBehaviorTrees.Conditions;
using EnemyBehaviorTrees.Decorators;

namespace EnemyBehaviorTrees.Agents
{
    public enum NavigationActivity
    {
        Waypoint, 
        PickupItem
    }

    public class BehaviorTreeTestNPCController : MonoBehaviour, IBehaviorTree
    {
        public NavMeshAgent MyNavMesh { get; private set; }
        public NavigationActivity MyActivity { get; set; }
        public NodeBase BehaviorTree { get; set; }

        private Coroutine behaviorTreeRoutine;
        private YieldInstruction waitTime = new WaitForSeconds(.1f);

        private void Start()
        {
            MyNavMesh = GetComponent<NavMeshAgent>();
            MyActivity = NavigationActivity.Waypoint;

            GenerateBehaviorTree();

            if (behaviorTreeRoutine == null && BehaviorTree != null)
            {
                behaviorTreeRoutine = StartCoroutine(RunBehaviorTree());
            }
        }

        private void GenerateBehaviorTree()
        {
            BehaviorTree = new Selector("Control NPC",
                                new Sequence("Pickup Item",
                                    new IsNavigationActivityTypeOf(NavigationActivity.PickupItem),
                                    new Selector("Look for or move to items",
                                        new Sequence("Look for items",
                                            new Inverter("Inverter",
                                                new AreItemsNearBy(5f)),
                                            new SetNavigationActivityTo(NavigationActivity.Waypoint)),
                                        new Sequence("Navigate to Item",
                                            new NavigateToDestination()))),
                                new Sequence("Move to Waypoint",
                                    new IsNavigationActivityTypeOf(NavigationActivity.Waypoint),
                                    new NavigateToDestination(),
                                    new Timer(2f,
                                        new SetNavigationActivityTo(NavigationActivity.PickupItem))));
        }

        private IEnumerator RunBehaviorTree()
        {
            while (enabled)
            {
                if (BehaviorTree == null)
                {
                    $"{this.GetType().Name} is missing Behavior Tree. Did you set the BehaviorTree property?".BTDebugLog();
                    continue;
                }

                (BehaviorTree as Node).Run();

                yield return waitTime;
            }
        }

        private void OnDestroy()
        {
            if (behaviorTreeRoutine != null)
            {
                StopCoroutine(behaviorTreeRoutine);
            }
        }

    }
}