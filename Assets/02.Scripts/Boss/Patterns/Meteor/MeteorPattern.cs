using System.Collections;
using UnityEngine;

public class MeteorPattern : BossPattern
{
    protected override IEnumerator Execute(BossContext ctx)
        {
            yield return null;
        }
}
