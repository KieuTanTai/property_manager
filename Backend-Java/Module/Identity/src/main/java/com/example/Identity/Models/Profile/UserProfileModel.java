package com.example.Identity.Models.Profile;

import java.time.LocalDateTime;
import java.util.Date;
import java.util.UUID;

import com.example.Shared.Enum.ESystemUserGender;

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
@Table(name = "user_profile")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class UserProfileModel {
    @Id
    @Column(name = "user_profile_id", nullable=false, length=12)
    private String userProfileId;

    @Column(name = "user_profile_account_id", nullable=false)
    private UUID userProfileAccountId;

    @Column(name = "user_profile_first_name", length=30)
    private String userProfileFirstName;

    @Column(name = "user_profile_last_name", length=30)
    private String userProfileLastName;

    @Column(name = "user_profile_date_of_birth")
    private Date userProfileDateOfBirth;

    @Column(name = "user_profile_gender", nullable=false)
    private ESystemUserGender userProfileGender;

    @Column(name = "user_profile_phone_number", length=10)
    private String userProfilePhoneNumber;

    @Column(name = "user_profile_address", nullable=false, length=255)
    private String userProfileAddress;

    @Column(name = "user_profile_avatar_url", length=255)
    private String userProfileAvatarUrl;

    @Column(name = "user_profile_created_at")
    private LocalDateTime userProfileCreatedAt;

    @Column(name = "user_profile_updated_at")
    private LocalDateTime userProfileUpdatedAt;
}
