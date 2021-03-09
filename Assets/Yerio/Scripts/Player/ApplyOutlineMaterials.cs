using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyOutlineMaterials : MonoBehaviour
{
    [SerializeField] GameObject playerController;
    [SerializeField] WaitForHostScreen waitForHostScreen;
    [Space]
    [SerializeField] Material team_TopOutline;
    [SerializeField] Material team_PantsOutline;
    [SerializeField] Material team_ShoesOutline;
    [Space]
    [SerializeField] Material enemy_TopOutline;
    [SerializeField] Material enemy_PantsOutline;
    [SerializeField] Material enemy_ShoesOutline;
    [Space]
    [SerializeField] SkinnedMeshRenderer topRenderer;
    [SerializeField] SkinnedMeshRenderer pantsRenderer;
    [SerializeField] SkinnedMeshRenderer shoesRenderer;

    bool materialsSet = false;

    public void ApplyMaterialsOnAllPlayers()
    {
        foreach (var player in waitForHostScreen.players)
        {
            var controller = player.GetComponentInChildren<PlayerController>();
            
            if(controller.gameObject.CompareTag(playerController.tag))
            {
                //team outline
                controller.GetComponent<ApplyOutlineMaterials>().ApplyTeamMaterialsOnThisPlayer();
            }
            else
            {
                //enemy outline
                controller.GetComponent<ApplyOutlineMaterials>().ApplyEnemyMaterialsOnThisPlayer();
            }
        }
    }

    public void ApplyTeamMaterialsOnThisPlayer()
    {
        if (!materialsSet)
        {
            topRenderer.material = team_TopOutline;
            pantsRenderer.material = team_PantsOutline;
            shoesRenderer.material = team_ShoesOutline;
            materialsSet = true;
        }
    }
    public void ApplyEnemyMaterialsOnThisPlayer()
    {
        if (!materialsSet)
        {
            topRenderer.material = enemy_TopOutline;
            pantsRenderer.material = enemy_PantsOutline;
            shoesRenderer.material = enemy_ShoesOutline;
            materialsSet = true;
        }
    }
}
