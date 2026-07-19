
void FUN_00733858(void)

{
  byte bVar1;
  ushort uVar2;
  int *piVar3;
  int iVar4;
  int iVar5;
  byte *pbVar6;
  byte *pbVar7;
  byte *pbVar8;
  short sVar9;
  uint uVar10;
  byte *pbVar11;
  uint uVar12;
  uint in_fpscr;
  float fVar13;
  float fVar14;
  longlong lVar15;
  uint local_3c;
  byte local_38;
  byte local_37;
  byte local_36;
  byte local_35;
  byte local_34;
  byte local_33;
  byte local_32;
  byte local_31 [13];
  
  lVar15 = FUN_007ffb80();
  piVar3 = (int *)lVar15;
  local_3c = 0;
  if (0 < DAT_00902ea0) {
    do {
      if ((local_3c & 0x1f) == 0) {
        iVar4 = FUN_0058ef48();
        fVar14 = (float)VectorSignedToFloat(local_3c,(byte)(in_fpscr >> 0x16) & 3);
        fVar13 = (float)VectorSignedToFloat((int)DAT_00902ea0,(byte)(in_fpscr >> 0x16) & 3);
        fVar14 = fVar14 / fVar13;
        uVar12 = in_fpscr & 0xfffffff | (uint)(fVar14 < 1.0) << 0x1f | (uint)(fVar14 == 1.0) << 0x1e
        ;
        in_fpscr = uVar12 | (uint)NAN(fVar14) << 0x1c;
        *(float *)(iVar4 + 0x2c10) = fVar14;
        bVar1 = (byte)(uVar12 >> 0x18);
        if (!(bool)(bVar1 >> 6 & 1) && bVar1 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar4 + 0x2c10) = 1.0;
        }
      }
      iVar4 = 0;
      pbVar11 = (byte *)(DAT_00902934 * local_3c * 0xe + DAT_00902928);
      if (0 < DAT_00902ea4) {
        do {
          *pbVar11 = 0;
          (**(code **)(*piVar3 + 0x1c))(piVar3,&local_37,1);
          bVar1 = (pbVar11[1] ^ local_37) & 1 ^ pbVar11[1];
          pbVar11[1] = bVar1;
          bVar1 = bVar1 ^ (bVar1 ^ local_37) & 2;
          pbVar11[1] = bVar1;
          bVar1 = bVar1 ^ (bVar1 ^ local_37) & 4;
          pbVar11[1] = bVar1;
          bVar1 = bVar1 ^ (bVar1 ^ local_37) & 0x18;
          pbVar11[1] = bVar1;
          bVar1 = local_37 ^ (bVar1 ^ local_37) & 0x7f;
          pbVar11[1] = bVar1;
          if ((bVar1 & 1) != 0) {
            if (lVar15 < 0x3b00000000) {
              (**(code **)(*piVar3 + 0x1c))(piVar3,local_31,1);
              *(ushort *)(pbVar11 + 6) = (ushort)local_31[0];
            }
            else {
              (**(code **)(*piVar3 + 0x1c))(piVar3,pbVar11 + 6,2);
            }
            uVar2 = *(ushort *)(pbVar11 + 6);
            uVar12 = (uint)uVar2;
            if (uVar12 == 0x7f) {
              pbVar11[1] = pbVar11[1] & 0xfe;
            }
            if (((&DAT_010262d0)[uVar12 * 4] & 0x10000) == 0) {
              sVar9 = -1;
              pbVar11[10] = 0xff;
              pbVar11[0xb] = 0xff;
              goto LAB_00733a08;
            }
            (**(code **)(*piVar3 + 0x1c))(piVar3,pbVar11 + 10,2);
            (**(code **)(*piVar3 + 0x1c))(piVar3,pbVar11 + 0xc,2);
            FUN_0072b00c(pbVar11,(int)((ulonglong)lVar15 >> 0x20));
            if (*(short *)(pbVar11 + 6) == 0x13) {
              if (*(short *)(pbVar11 + 0xc) < 0) {
                pbVar11[0xc] = 0;
                pbVar11[0xd] = 0;
              }
            }
            else if (*(short *)(pbVar11 + 6) == 0x90) {
              pbVar11[0xc] = 0;
              pbVar11[0xd] = 0;
            }
            else if (lVar15 < 0x3a00000000) {
              if (uVar12 == 0x23) {
                sVar9 = *(short *)(pbVar11 + 0xc) + 0x36;
              }
              else {
                if (uVar12 != 0x24) goto LAB_00733a0a;
                sVar9 = *(short *)(pbVar11 + 0xc) + 0x6c;
              }
LAB_00733a08:
              *(short *)(pbVar11 + 0xc) = sVar9;
            }
LAB_00733a0a:
            *(ushort *)(pbVar11 + 6) = uVar2;
            (**(code **)(*piVar3 + 0x1c))(piVar3,&local_35,1);
            *(ushort *)(pbVar11 + 2) =
                 (*(ushort *)(pbVar11 + 2) ^ (ushort)local_35) & 0x1f ^ *(ushort *)(pbVar11 + 2);
          }
          (**(code **)(*piVar3 + 0x1c))(piVar3,&local_36,1);
          if (local_36 != 0) {
            (**(code **)(*piVar3 + 0x1c))(piVar3,&local_32,1);
            *(ushort *)(pbVar11 + 2) =
                 (*(ushort *)(pbVar11 + 2) ^ (ushort)local_32 << 5) & 0x3e0 ^
                 *(ushort *)(pbVar11 + 2);
          }
          pbVar11[8] = local_36;
          (**(code **)(*piVar3 + 0x1c))(piVar3,pbVar11 + 4,1);
          if (pbVar11[4] != 0) {
            (**(code **)(*piVar3 + 0x1c))(piVar3,&local_34,1);
            *(ushort *)(pbVar11 + 2) =
                 *(ushort *)(pbVar11 + 2) ^
                 (*(ushort *)(pbVar11 + 2) ^ (ushort)local_34 << 0xc) & 0x3000;
          }
          (**(code **)(*piVar3 + 0x1c))(piVar3,&local_38,1);
          *pbVar11 = local_38 & 0x18 | *pbVar11;
          uVar2 = (*(ushort *)(pbVar11 + 2) ^ (ushort)local_38 << 5) & 0x400 ^
                  *(ushort *)(pbVar11 + 2);
          *(ushort *)(pbVar11 + 2) = uVar2;
          *(ushort *)(pbVar11 + 2) = uVar2 ^ (uVar2 ^ (ushort)local_38 << 5) & 0x800;
          if (0x3bffffffff < lVar15) {
            (**(code **)(*piVar3 + 0x1c))(piVar3,&local_33,1);
            pbVar11[5] = local_33;
          }
          iVar5 = FUN_007a7d34();
          if (iVar5 != 0) {
            *pbVar11 = *pbVar11 & 0xf7;
          }
          (**(code **)(*piVar3 + 0x1c))(piVar3,&local_38,1);
          uVar12 = (uint)local_38;
          if ((local_38 & 0x80) != 0) {
            (**(code **)(*piVar3 + 0x1c))(piVar3,&local_38,1);
            uVar12 = uVar12 & 0x7f | (uint)local_38 << 7;
          }
          if (uVar12 != 0) {
            uVar10 = uVar12 * 0xe >> 1;
            if (uVar10 != 0) {
              pbVar6 = pbVar11;
              pbVar7 = pbVar11 + 0xe;
              do {
                pbVar8 = pbVar7 + 2;
                *(undefined2 *)pbVar7 = *(undefined2 *)pbVar6;
                pbVar6 = pbVar6 + 2;
                pbVar7 = pbVar8;
              } while (pbVar8 != pbVar11 + 0xe + uVar10 * 2);
            }
            pbVar11 = pbVar11 + uVar12 * 0xe;
          }
          iVar4 = iVar4 + uVar12 + 1;
          pbVar11 = pbVar11 + 0xe;
        } while (iVar4 < DAT_00902ea4);
      }
      local_3c = local_3c + 1;
    } while ((int)local_3c < (int)DAT_00902ea0);
  }
  FUN_007ffb98();
  return;
}

