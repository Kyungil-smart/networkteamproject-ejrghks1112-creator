using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxUI : MonoBehaviour
{
    public List<VivoxParticipant> VivoxParticipants = new();       // 음성채널에 참가하고 있는 참가자의 정보들을 모아놓은 List 
    public List<VoiceChatPlayerSlot> voiceChatPlayerSlots = new(); // 참가자들의 음성채팅 관련 옵션을 조절하기 위해 사용할 List

    // 자신을 제외한 참가자들의 정보를 각각 voiceChatPlayerSlots에 연계하여 옵션을 조절할 수 있게끔 하는 메서드
    // 채널에 참가하거나, 퇴장하여 인원이 변경될 때마다 호출 됨
    // 몇 번째 인원이 참가하거나, 퇴장해도 옵션창의 순서가 보장되도록 voiceChatPlayerSlots를 리셋하고 다시 연계
    public void RefreshUI()
    {
        foreach (VoiceChatPlayerSlot slot in voiceChatPlayerSlots)
        {
            slot.gameObject.SetActive(false);
        }

        voiceChatPlayerSlots.Clear();
        
        int slotIndex = 0;
        
        foreach (VivoxParticipant participant in VivoxParticipants)
        {
             if (participant.IsSelf) continue;
            
            if (slotIndex >= voiceChatPlayerSlots.Count) break;

            voiceChatPlayerSlots[slotIndex].SetParticipant(participant);

            voiceChatPlayerSlots[slotIndex].gameObject.SetActive(true);

            slotIndex++;
        }
    }
}
