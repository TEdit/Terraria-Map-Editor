
void FUN_00733e90(void)

{
  uint uVar1;
  ushort uVar2;
  char cVar3;
  ushort uVar4;
  int *piVar5;
  int iVar6;
  uint *puVar7;
  int iVar8;
  uint uVar9;
  byte bVar10;
  int iVar11;
  int iVar12;
  uint uVar13;
  int iVar14;
  int iVar15;
  uint uVar16;
  uint in_fpscr;
  float fVar17;
  float fVar18;
  undefined4 local_44;
  undefined4 local_40;
  undefined4 local_3c;
  ushort local_38;
  undefined2 local_34;
  
  piVar5 = (int *)FUN_007ffb80();
  uVar16 = 0;
  if (0 < DAT_00902ea0) {
    do {
      if ((uVar16 & 0x1f) == 0) {
        iVar6 = FUN_0058ef48();
        fVar18 = (float)VectorSignedToFloat(uVar16,(byte)(in_fpscr >> 0x16) & 3);
        fVar17 = (float)VectorSignedToFloat((int)DAT_00902ea0,(byte)(in_fpscr >> 0x16) & 3);
        fVar18 = fVar18 / fVar17;
        uVar1 = in_fpscr & 0xfffffff | (uint)(fVar18 < 1.0) << 0x1f | (uint)(fVar18 == 1.0) << 0x1e;
        in_fpscr = uVar1 | (uint)NAN(fVar18) << 0x1c;
        *(float *)(iVar6 + 0x2c10) = fVar18;
        bVar10 = (byte)(uVar1 >> 0x18);
        if (!(bool)(bVar10 >> 6 & 1) && bVar10 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar6 + 0x2c10) = 1.0;
        }
      }
      iVar6 = 0;
      if (0 < DAT_00902ea4) {
        do {
          puVar7 = (uint *)((DAT_00902934 * uVar16 + iVar6) * 0xe + DAT_00902928);
          uVar9 = *puVar7;
          local_3c = puVar7[2];
          uVar1 = puVar7[3];
          local_40._2_2_ = (ushort)(puVar7[1] >> 0x10);
          uVar4 = local_40._2_2_;
          uVar13 = (uint)local_40._2_2_;
          local_44._1_1_ = (byte)(uVar9 >> 8);
          bVar10 = local_44._1_1_;
          if (uVar13 == 0x7f) {
            bVar10 = local_44._1_1_ & 0xfe;
            local_44 = uVar9 & 0xfffffeff;
            uVar9 = local_44;
          }
          local_44 = uVar9;
          local_34 = CONCAT11(local_34._1_1_,bVar10) & 0xff9f;
          local_40 = puVar7[1];
          local_38 = (ushort)uVar1;
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          uVar9 = local_44;
          uVar2 = local_44._2_2_;
          if ((bVar10 & 1) != 0) {
            local_34 = uVar4;
            (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,2);
            if (((&DAT_010262d0)[uVar13 * 4] & 0x10000) != 0) {
              local_34 = local_3c._2_2_;
              (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,2);
              local_34 = (ushort)uVar1;
              (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,2);
            }
            local_34 = CONCAT11(local_34._1_1_,(char)(uVar9 >> 0x10)) & 0xff1f;
            (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          }
          cVar3 = (char)local_3c;
          local_34._0_1_ = (char)local_3c;
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          if (cVar3 != '\0') {
            local_34._0_1_ = (byte)(uVar2 >> 5) & 0x1f;
            (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          }
          cVar3 = (char)local_40;
          local_34._0_1_ = (char)local_40;
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          if (cVar3 != '\0') {
            local_34._0_1_ = (byte)(uVar9 >> 0x1c) & 3;
            (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          }
          local_34._0_1_ =
               (byte)local_44 & 0x18 | (byte)((uVar2 >> 10 & 1 | (uVar2 >> 0xb & 1) << 1) << 5);
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          local_34 = CONCAT11(local_34._1_1_,local_40._1_1_);
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          iVar11 = 1;
          iVar14 = iVar6 + 1;
          iVar15 = (int)DAT_00902ea4;
          if (iVar14 < iVar15) {
            iVar12 = (DAT_00902934 * uVar16 + iVar6 + 1) * 0xe + DAT_00902928;
            do {
              iVar8 = FUN_0072b574(&local_44,iVar12);
              if (iVar8 == 0) break;
              iVar14 = iVar14 + 1;
              iVar11 = iVar11 + 1;
              iVar12 = iVar12 + 0xe;
            } while (iVar14 < iVar15);
          }
          iVar11 = iVar11 + -1;
          if (iVar11 < 0x80) {
            local_34 = CONCAT11(local_34._1_1_,(char)iVar11);
          }
          else {
            local_34 = CONCAT11(local_34._1_1_,(char)iVar11) | 0x80;
            (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
            local_34 = CONCAT11(local_34._1_1_,(char)(iVar11 >> 7));
          }
          (**(code **)(*piVar5 + 0x18))(piVar5,&local_34,1);
          iVar6 = iVar6 + iVar11 + 1;
        } while (iVar6 < DAT_00902ea4);
      }
      uVar16 = uVar16 + 1;
    } while ((int)uVar16 < (int)DAT_00902ea0);
  }
  FUN_007ffb98();
  return;
}

