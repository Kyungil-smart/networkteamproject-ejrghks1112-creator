using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.Services.Vivox;
using UnityEngine;

/// <summary>
/// 음성채팅 옵션창
/// </summary>
public class VivoxUI : MonoBehaviour
{
    public List<VivoxParticipant> VivoxParticipants = new();       // 음성채널에 참가하고 있는 참가자의 정보들을 모아놓은 List 
    public List<VoiceChatPlayerSlot> voiceChatPlayerSlots = new(); // 참가자들의 음성채팅 관련 옵션을 조절하기 위해 사용할 Slot List

    /// <summary>
    /// 자신을 제외한 참가자들의 정보를 각각 voiceChatPlayerSlots에 연동하여 옵션을 조절할 수 있게끔 하는 메서드
    /// 채널에 참가하거나, 퇴장하여 인원이 변경될 때마다 호출 됨
    /// 옵션창에 a,b,c라는 참가자들이 슬롯에 담겨 있을 때 b가 나가게 될 경우, a와c 사이에 빈칸 발생, 이럴 경우 빈칸 없이 a,c가 순서대로
    /// 나열 될 수 있게끔 호출될 때마다 전부 SetActive(false)하고 인덱스 0부터 다시 연동
    /// </summary>
    public void RefreshUI()
    {
        // 호출될 때 슬롯들 전부 SetActive(false), 
        foreach (VoiceChatPlayerSlot slot in voiceChatPlayerSlots)
        {
            slot.gameObject.SetActive(false);
        }
        
        int slotIndex = 0;
        
        foreach (VivoxParticipant participant in VivoxParticipants)
        {
            if (participant.IsSelf) continue; // 자신은 추가하지 않음
            
            if (slotIndex >= voiceChatPlayerSlots.Count) break; 

            // voiceChatPlayerSlots에 참가자 연동 
            voiceChatPlayerSlots[slotIndex].SetParticipant(participant); 

            // 연동 했으면 SetActive(true);
            voiceChatPlayerSlots[slotIndex].gameObject.SetActive(true);

            slotIndex++;
        }
    }
}
