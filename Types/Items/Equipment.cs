using System.Text.Json.Serialization;

namespace Arcatos.Types.Items
{
    public struct EquipmentDto
    {
        [JsonInclude] public          string   name;
        [JsonInclude] public required string   summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required string   slot;
        [JsonInclude] public required int      layer;

    }
    
    public class Equipment : Item
    {
        public EqSlot                  Slot  { get; init; }
        public int                     Layer { get; init; }

        public Equipment(string id, EntityDto dto) : base(id, dto.Name!, dto.Summary, dto.Description)
        {
            this.IsConsumable = false;
            this.Slot = Enum.TryParse(dto.Slot, out EqSlot slot) ? slot : throw new ArgumentException($"Slot {dto.Slot} is not supported");
            this.Layer      = dto.Layer ?? throw new ArgumentException($"Equipment {id} does not have a valid layer");
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
        shoulders, rightShoulder, leftShoulder,
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
