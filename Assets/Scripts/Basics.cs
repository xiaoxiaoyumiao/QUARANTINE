using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum VarType
{
    HEALTHY_POP,
    INFECTED_POP_CURR_GEN,
    INFECTED_POP_NEXT_GEN,
    MATERIAL
}

/* class Variable
 * 这是为了方便批量迭代而写的数据封装
 * 读写Data域可以直接获取和修改值，DataBuf域会保持和Data域的一致
 * 写DataBuf域可以将要写的值缓存，避免覆盖掉之后可能还要用到的Data
 * 然后调用Commit就可以将DataBuf的值更新到Data
 */
public class Variable<T>
{
    private T data;
    private T dataBuffer;
    private VarType type;
    private bool dirty;

    public T Data
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
            dataBuffer = value;
            dirty = false;
        }
    }
    public T DataBuf
    {
        get
        {
            return dataBuffer;
        }
        set
        {
            dataBuffer = value;
            dirty = true;
        }
    }

    public Variable(T mData, VarType mType)
    {
        data = mData;
        type = mType;
        dirty = false;
    }
    public bool NeedCommit()
    {
        return dirty;
    }
    public void Commit()
    {
        data = dataBuffer;
        dirty = false;
    }

}

public class InfectedStage
{
    public static readonly InfectedStage[] stages = { 
        new InfectedStage(0.0f, 0.0f),
        new InfectedStage(1.0f, 0.0f),
        new InfectedStage(1.0f, 0.1f),
        new InfectedStage(1.0f, 0.5f)
    };
    public float reproduction; // R0
    public float deathRate;
    public InfectedStage(float mReproduction, float mDeathRate){
        reproduction = mReproduction;
        deathRate = mDeathRate;
    }
}

public class InfectedTimer
{
    public readonly int period; // T = 3D
    int timer;
    int stagePointer;
    public InfectedTimer(int mPeriod)
    {
        period = mPeriod;
        stagePointer = 0;
    }

    public void Tick()
    {
        timer++;
        if (timer == period)
        {
            timer = 0;
            stagePointer = (stagePointer+1)%4;
        }
    }

    public float Reproduction
    {
        get
        {
            return InfectedStage.stages[stagePointer].reproduction;
        }
    }

    public float DeathRate
    {
        get
        {
            return InfectedStage.stages[stagePointer].deathRate;
        }
    }
    
}

public class Block
{
    public BlockType type;

    readonly float R0 = 2.0f;
    readonly float DR = 0.1f;

    public bool isWorking = false;
    readonly float MCR = -0.01f; // material count rate
    readonly float WMCR = +1f; // material count rate FOR FACTORY
    public float MCRate
    {
        get
        {
            if (isWorking) return WMCR;
            else return MCR;
        }

    }


    readonly int RESOURCE_MIN = 0;
    readonly float taxRate = 0.05f;
    readonly int QUARANTINE_PERIOD = 10;
    public GameObject blockUI;

    // const int MAX_OUT_BLOCK = 4;
    public List<Block> outBlocks; // Blocks

    public Variable<int> HPCount;
    public Variable<int> CIPCount; // Current generation of infected
    public Variable<int> NIPCount; // Next generation of infected, temporarily not used

    public Variable<int> MaterialCount;

    public Block(GameObject mBlockUI)
    {
        blockUI = mBlockUI;
        Landblock lb = blockUI.GetComponent<Landblock>();
        type = lb.type;

        if (type == BlockType.FACTORY)
        {
            isWorking = true;
        }
        lb.block = this;

        outBlocks = new List<Block>();

        HPCount = new Variable<int>(0, VarType.HEALTHY_POP);
        CIPCount = new Variable<int>(0, VarType.INFECTED_POP_CURR_GEN);
        // NIPCount = new Variable<int>(0, VarType.INFECTED_POP_NEXT_GEN);

        MaterialCount = new Variable<int>(0, VarType.MATERIAL);

        // initialize infected population
        HPIPInit(lb.infected);
        MCInit(lb.material);
    }

    public void AddOutBlock(Block target)
    {
        outBlocks.Add(target);
    }

    public void HPIPInit(int infected)
    {
        HPCount.Data = Random.Range(400, 600);
        CIPCount.Data = infected;
    }

    public void MCInit(int material)
    {
        MaterialCount.Data = material;
    }

    public void Commit()
    {
        HPCount.Commit();
        CIPCount.Commit();
        MaterialCount.Commit();
    }

    /* 以下方法段是一些与父对象互动的工具方法 */
    public void stopWorking()
    {
        isWorking = false;   
    }
    public void startWorking()
    {
        if (type == BlockType.FACTORY)
            isWorking = true;
    }

    public int taxed()
    {
        int taxed = (int)System.Math.Floor(MaterialCount.Data * taxRate);
        MaterialCount.Data -= taxed;
        return taxed;
    }

    bool isQuarantined;
    int quarantineCounter;
    public void quarantined()
    {
        isQuarantined = true;
        quarantineCounter = 0;
    }

    public void aided()
    {
        HPCount.Data = 0;
        CIPCount.Data = 0;
    }

    /* 回合结束时首先执行的块内结算，不涉及与其他块的数据交换 */
    public void EndInBlock()
    {
        // population counted
        int inf = (int)(Random.Range(0.0f, R0) * CIPCount.Data);
        if (inf > HPCount.Data) inf = HPCount.Data;
        CIPCount.Data += inf;
        HPCount.Data -= inf;

        int death = (int)(Random.Range(0.0f, DR) * CIPCount.Data);
        CIPCount.Data -= death;

        // material counted
        MaterialCount.Data += (int)System.Math.Floor(MCRate*(HPCount.Data+CIPCount.Data));
        if (MaterialCount.Data < RESOURCE_MIN)
            MaterialCount.Data = RESOURCE_MIN;
    }

    /* 这一阶段涉及的数据计算一般需要依赖于来自其他区块的数据 */
    public void EndRound()
    {
        if (isQuarantined)
        {
            quarantineCounter += 1;
            if (quarantineCounter == QUARANTINE_PERIOD)
            {
                isQuarantined = false;
            }
            return;
        }
        int len = outBlocks.Count;
        int distr = HPCount.Data / len / 2;
        int idistr = CIPCount.Data / len / 2;
        foreach (Block target in outBlocks)
        {
            target.HPCount.DataBuf += distr;
            HPCount.DataBuf -= distr;
            target.CIPCount.DataBuf += idistr;
            CIPCount.DataBuf -= idistr;
        }
    }
}