using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum VarType
{
    HEALTHY_POP,
    INFECTED_POP
}

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

public class Block
{
    const float R0 = 2.0f;
    const float DR = 0.1f;
    public GameObject blockUI;

    // const int MAX_OUT_BLOCK = 4;
    public List<Block> outBlocks; // Blocks

    public Variable<int> HPCount;
    public Variable<int> IPCount;

    public Block(GameObject mBlockUI)
    {
        blockUI = mBlockUI;
        blockUI.GetComponent<Landblock>().block = this;

        outBlocks = new List<Block>();

        HPCount = new Variable<int>(0, VarType.HEALTHY_POP);
        IPCount = new Variable<int>(0, VarType.INFECTED_POP);
    }

    public void AddOutBlock(Block target)
    {
        outBlocks.Add(target);
    }

    public void HPIPInit(int infected)
    {
        HPCount.DataBuf = Random.Range(400, 600);
        IPCount.DataBuf = infected;
        Commit();
    }

    public void Commit()
    {
        HPCount.Commit();
        IPCount.Commit();
    }

    public void EndInBlock()
    {
        int inf = (int)(Random.Range(0.0f, R0) * IPCount.Data);
        if (inf > HPCount.Data) inf = HPCount.Data;
        IPCount.Data += inf;
        HPCount.Data -= inf;

        int death = (int)(Random.Range(0.0f, DR) * IPCount.Data);
        IPCount.Data -= death;
    }

    public void EndRound()
    {        
        int len = outBlocks.Count;
        int distr = HPCount.Data / len / 2;
        int idistr = IPCount.Data / len / 2;
        foreach (Block target in outBlocks)
        {
            target.HPCount.DataBuf += distr;
            HPCount.DataBuf -= distr;
            target.IPCount.DataBuf += idistr;
            IPCount.DataBuf -= idistr;
        }
    }
}
