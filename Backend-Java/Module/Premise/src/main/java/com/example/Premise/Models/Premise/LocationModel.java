package com.example.Premise.Models.Premise;

import java.time.LocalDateTime;
import java.util.UUID;

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
@Table(name = "location")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class LocationModel {
    @Id 
    @Column(name = "location_id", nullable = false)
    private UUID locationId;

    @Column(name = "location_address", nullable = false, length = 255)
    private String locationAddress;

    @Column(name ="location_created_at")
    private LocalDateTime locationCreatedAt;

    @Column(name = "location_updated_at")
    private LocalDateTime locationUpdatedAt;
}
