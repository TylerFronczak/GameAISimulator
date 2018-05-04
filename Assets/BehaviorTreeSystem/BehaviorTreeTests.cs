//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using NUnit.Framework;
using BehaviorTreeSystem;
using BehaviorTreeSystem.Enums;

public class BehaviorTreeTests
{
    [Test] public void Action_Success()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Action(new MockAction(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }
    [Test] public void Action_Failure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Action(new MockAction(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }

	[Test] public void Condition_Success()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
	}
    [Test] public void Condition_Failure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }

    [Test] public void Decorator_Invert_ChildSuccess()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Decorator(new Invert())
                .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
    [Test] public void Decorator_Invert_ChildFailure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Decorator(new Invert())
                .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }

    [Test] public void Decorator_Repeat_ChildSuccess()
    {
        Repeat repeat = new Repeat(10);

        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Decorator(repeat)
                .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
        Assert.IsTrue(repeat.repetitionCount == 10);
    }
    [Test] public void Decorator_Repeat_ChildFailure()
    {
        Repeat repeat = new Repeat(10);

        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Decorator(repeat)
                .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
        Assert.IsTrue(repeat.repetitionCount == 0);
    }

    [Test] public void Selector_BothChildrenSuccess()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Success))
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }
    [Test] public void Selector_BothChildrenFailure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Failure))
            .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
    [Test] public void Selector_FirstChildSuccessAndNextChildFailure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Success))
            .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }
    [Test] public void Selector_FirstChildFailureAndNextChildSuccess()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Condition(new MockCondition(BehaviorStatus.Failure))
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }
    [Test] public void Selector_FirstChildRunningAndNextChildSuccess()
    {
        MockAction mockActionA = new MockAction(BehaviorStatus.Running);

        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Action(mockActionA)
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Running);

        mockActionA.status = BehaviorStatus.Failure;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }

    [Test] public void Sequence_BothChildrenSuccess()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Sequence())
            .Condition(new MockCondition(BehaviorStatus.Success))
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);
    }
    [Test] public void Sequence_BothChildrenFailure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Sequence())
            .Condition(new MockCondition(BehaviorStatus.Failure))
            .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
    [Test] public void Sequence_FirstChildSuccessAndNextChildFailure()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Sequence())
            .Condition(new MockCondition(BehaviorStatus.Success))
            .Condition(new MockCondition(BehaviorStatus.Failure))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
    [Test] public void Sequence_FirstChildFailureAndNextChildSuccess()
    {
        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Sequence())
            .Condition(new MockCondition(BehaviorStatus.Failure))
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
    [Test] public void Sequence_FirstChildRunningAndNextChildSuccess()
    {
        MockAction mockActionA = new MockAction(BehaviorStatus.Running);

        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Sequence())
            .Action(mockActionA)
            .Condition(new MockCondition(BehaviorStatus.Success))
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Running);

        mockActionA.status = BehaviorStatus.Failure;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }

    [Test] public void TreeA()
    {
        MockCondition isSafe = new MockCondition(BehaviorStatus.Failure);
        MockCondition isFoodNearby = new MockCondition(BehaviorStatus.Success);
        MockAction run = new MockAction(BehaviorStatus.Running);
        MockAction eat = new MockAction(BehaviorStatus.Running);

        BehaviorTree behaviorTree = new BehaviorTreeBuilder<BehaviorTree>(new Selector())
            .Sequence("Run")
                .Decorator(new Invert())
                    .Condition(isSafe)
                .Action(run)
            .End()
            .Sequence("Eat")
                .Condition(isFoodNearby)
                .Action(eat)
            .End()
        .Build();

        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Running);

        run.status = BehaviorStatus.Success;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);

        isSafe.status = BehaviorStatus.Success;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Running);

        eat.status = BehaviorStatus.Success;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Success);

        isFoodNearby.status = BehaviorStatus.Failure;
        Assert.IsTrue(behaviorTree.Tick() == BehaviorStatus.Failure);
    }
}
