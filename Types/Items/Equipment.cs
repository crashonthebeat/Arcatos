using System.Text.Json.Serialization;

namespace Arcatos.Types.Items
{
    public struct EquipmentDto
    {
        [JsonInclude] public          string   name;
        [JsonInclude] public required string   summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required Dictionary<string, int> slots;

    }
    
    public class Equipment : Item
    {
        public Dictionary<EqSlot, int> Slots  { get; init; }
        
        public Equipment(string id, EntityDto dto) : base(id, dto.Name!, dto.Summary, dto.Description)
        {
            this.IsConsumable = false;
            this.Slots        = new Dictionary<EqSlot, int>();
            
            foreach (KeyValuePair<string, int> kvp in dto.Slots!)
            {
                EqSlot slotName = Enum.TryParse(kvp.Key, out EqSlot slot) ? slot : throw new ArgumentException($"Slot {kvp.Key} is not supported");
                this.Slots.Add(slotName, kvp.Value);
            }
        }
    }
    
    // Equipment Slots are meant to be exhaustive of possible slots for equipment.
    // Devs can use as many or as few as they want, as this system is meant to be fairly free-form.
    public enum EqSlot
    {
        // Head and Neck
        head,
        face,
        eyes,
        neck,
        ears, rightEar, leftEar,
        // Torso
        rightShoulder, leftShoulder,
        chest,
        torso,
        back,
        waist,
        hips,
        groin,
        // Arms
        arms, rightArm, leftArm,
        biceps, rightBicep, leftBicep,
        elbows, rightElbow, leftElbow,
        forearms, rightForearm, leftForearm,
        wrists, rightWrist, leftWrist,
        // Hands
        hands, rightHand, leftHand,
        fingers, rightFinger, leftFinger,
        rightFgrIndex, rightFgrMiddle, rightFgrRing, rightFgrPinky, rightThumb,
        leftFgrIndex, leftFgrMiddle, leftFgrRing, leftFgrPinky, leftThumb,
        // Legs
        legs, rightLeg, leftLeg,
        thighs, rightThigh, leftThigh,
        knees, rightKnee, leftKnee,
        calves, rightCalf, leftCalf,
        feet, rightFoot, leftFoot,
        ankles, rightAnkle, leftAnkle,
    }
}
