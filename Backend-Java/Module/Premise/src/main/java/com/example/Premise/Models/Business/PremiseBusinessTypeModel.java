package com.example.Premise.Models.Business;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "premise_business_type")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class PremiseBusinessTypeModel implements Serializable{
    @EmbeddedId
    private PremiseBusinessTypeId id;

    @Column(name = "pre_bt_assigned_at")
    private LocalDateTime assignedAt;

    @Embeddable
    @Getter
    @Setter
    @NoArgsConstructor
    @AllArgsConstructor
    @EqualsAndHashCode
    public static class PremiseBusinessTypeId implements Serializable {
        @Column(name = "pre_bt_premise_id", nullable = false)
        private UUID premiseId;

        @Column(name = "pre_bt_business_type_id", nullable = false)
        private UUID businessTypeId;
    }
}
