
void FUN_0078e920(void)

{
  undefined4 uVar1;
  void *pvVar2;
  int *piVar3;
  int iVar4;
  int *piVar5;
  int iVar6;
  int iVar7;
  int *piVar8;
  undefined4 uVar9;
  int local_6c;
  undefined1 auStack_68 [4];
  undefined1 auStack_64 [68];
  
  uVar1 = FUN_0082a0b8();
  pvVar2 = malloc(0x58);
  if (pvVar2 == (void *)0x0) {
    piVar3 = (int *)0x0;
  }
  else {
    *(undefined4 *)((int)pvVar2 + 0x3c) = 0;
    piVar3 = (int *)FUN_0057485c(pvVar2,uVar1,0,1,pvVar2);
  }
  iVar4 = (**(code **)(*piVar3 + 0x30))(piVar3);
  pvVar2 = malloc(0x9c);
  if (pvVar2 == (void *)0x0) {
    piVar5 = (int *)0x0;
  }
  else {
    *(undefined4 *)((int)pvVar2 + 0x3c) = 0;
    *(undefined4 *)((int)pvVar2 + 0x88) = 0;
    piVar5 = (int *)FUN_005f544c(pvVar2,iVar4,0);
  }
  uVar1 = (**(code **)(*piVar5 + 4))(piVar5);
  (**(code **)(*piVar3 + 0x24))(piVar3,uVar1,iVar4,*(code **)(*piVar3 + 0x24),pvVar2);
  uVar1 = (**(code **)(*piVar5 + 0x48))(piVar5);
  (**(code **)(*piVar5 + 8))(piVar5,uVar1);
  iVar6 = FUN_0078d7b4(piVar5);
  iVar7 = (**(code **)(*piVar5 + 0x2c))(piVar5);
  if (0x41 < iVar6) {
    piVar8 = (int *)FUN_007783c8(piVar5,iVar4 - iVar7);
    (**(code **)(*piVar8 + 0x48))();
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar5 + 0x18))(piVar5,iVar7,0);
    uVar1 = (**(code **)(*piVar8 + 0x48))(piVar8);
    uVar9 = (**(code **)(*piVar8 + 4))(piVar8);
    (**(code **)(*piVar5 + 0x20))(piVar5,uVar9,uVar1);
    uVar1 = (**(code **)(*piVar5 + 0x2c))(piVar5);
    (**(code **)(*piVar5 + 8))(piVar5,uVar1);
    (**(code **)*piVar8)(piVar8,1);
  }
  (**(code **)*piVar3)(piVar3,1);
  (**(code **)(*piVar5 + 0x18))(piVar5,0,0);
  iVar4 = (**(code **)(*piVar5 + 0x30))(piVar5);
  iVar6 = FUN_004f7cc0(0,0,0);
  (**(code **)(*piVar5 + 0x24))(piVar5,auStack_64,8);
  for (iVar4 = iVar4 + -8; 0 < iVar4; iVar4 = iVar4 - iVar7) {
    iVar7 = iVar4;
    if (0x40 < iVar4) {
      iVar7 = 0x40;
    }
    (**(code **)(*piVar5 + 0x24))(piVar5,auStack_64,iVar7);
    iVar6 = FUN_004f7cc0(iVar6,auStack_64,iVar7);
  }
  (**(code **)(*piVar5 + 0x18))(piVar5,0,0);
  (**(code **)(*piVar5 + 0x24))(piVar5,auStack_68,4);
  (**(code **)(*piVar5 + 0x24))(piVar5,&local_6c,4);
  (**(code **)*piVar5)(piVar5,1);
  FUN_0082a0d0(iVar6 == local_6c);
  return;
}

