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
@Table(name = "premise_media")
@Getter
@Setter
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class PremiseMediaModel {
    @Id 
    @Column(name = "premise_media_id", nullable = false)
    private int premiseMediaId;

    @Column(name = "premise_media_premise_id", nullable = false)
    private UUID premiseMediapremiseId;

    @Column(name = "premise_media_image_url", nullable = false, length = 255)
    private String premiseMediaImageUrl;

    @Column(name = "premise_media_created_at")
    private LocalDateTime premiseMediaCreatedAt;

    @Column(name = "premise_media_updated_at")
    private LocalDateTime premiseMediaUpdatedAt;
}
