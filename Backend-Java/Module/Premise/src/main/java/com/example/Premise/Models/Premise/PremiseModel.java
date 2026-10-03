package com.example.Premise.Models.Premise;

import java.time.LocalDateTime;
import java.util.UUID;

import com.example.Shared.Enum.EPremiseStatus;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "premise")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder
public class PremiseModel {
    @Id 
    @Column(name = "premise_id", nullable = false)
    private UUID premiseId;

    @Column(name = "premise_name", nullable = false, length = 50)
    private String premiseName;

    @Column(name = "premise_location_id", nullable = false)
    private UUID premiseLocationId;

    @Column(name = "premise_status", nullable = false)
    private EPremiseStatus premiseStatus;

    @Column(name = "premise_position", nullable = false)
    private int premisePosition;

    @Column(name = "premise_floor", nullable = false)
    private int premiseFloor;

    @Column(name = "premise_area", nullable = false, length = 10)
    private String premiseArea;

    @Column(name = "premise_description", length = 255)
    private String premiseDescription;

    @Column(name = "premise_created_at")
    private LocalDateTime premiseCreatedAt;

    @Column(name = "premise_updated_at")
    private LocalDateTime premiseUpdatedAt;
}
