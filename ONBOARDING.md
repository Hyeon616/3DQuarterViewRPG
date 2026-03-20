# ONBOARDING.md

프로젝트 온보딩 및 기획 문서

---

## 프로젝트 개요

**장르**: 3D 쿼터뷰 액션 RPG (마비노기 모바일 스타일)
**핵심 컨셉**: 무기 교체 = 직업 변경

### 게임 특징
- 무기를 바꾸면 직업이 변경됨 (고정 직업 없음)
- 각 무기 타입별 고유한 스킬셋과 플레이 스타일
- StatTree를 통한 캐릭터 커스터마이징
- 멀티플레이어 지원 (Mirror)

---

## 현재 구현 상태

### 완료된 시스템
- [x] 플레이어 이동 및 카메라
- [x] PlayableGraph 기반 애니메이션
- [x] 콤보 공격 시스템
- [x] 데미지 계산 (모디파이어 체인)
- [x] 히트 판정 (백어택/헤드어택)
- [x] StatTree UI 및 투자 시스템
- [x] 장비 시스템 (WeaponData/ArmorData)
- [x] 타입별 배율 시스템 (WeaponTypeData/ArmorTypeData)
- [x] 플레이어 데이터 저장 (AES 암호화)
- [x] 이펙트/사운드 풀링
- [x] 데미지 텍스트 UI

### 진행 중
- [ ] Unity에서 장비 데이터 에셋 생성

### 미구현
- [ ] 인벤토리 시스템
- [ ] 무기별 스킬셋 분리
- [ ] 몬스터 AI
- [ ] 던전/맵 시스템
- [ ] 퀘스트 시스템

---

## 기획 노트

### 무기 타입 (WeaponType)
| 타입 | 특징 | 배율 예시 |
|------|------|-----------|
| TwoHandedSword | 높은 공격력, 느린 속도 | ATK 1.3x, SPD 0.8x |
| SwordAndShield | 균형, 방어 가능 | ATK 1.0x, DEF 1.2x |
| Spear | 긴 사거리, 찌르기 | ATK 1.1x, Range 1.5x |
| Axe | 높은 치명타 | ATK 1.2x, CritDMG 1.3x |
| Hammer | 스태거 특화 | ATK 1.4x, SPD 0.6x |

### 방어구 타입 (ArmorType)
| 타입 | 특징 | 배율 예시 |
|------|------|-----------|
| Leather | 경량, 회피 | HP 0.8x, SPD 1.1x |
| Plate | 중장, 방어 | HP 1.3x, DEF 1.5x |
| Light | 밸런스 | HP 1.0x, DEF 1.0x |
| Heavy | 최고 방어 | HP 1.5x, DEF 1.8x, SPD 0.7x |

---

## 작업 요청 형식

새로운 기능이나 수정이 필요할 때 아래 형식으로 작성:

```
### [기능명]
**우선순위**: 높음/중간/낮음
**설명**:
무엇을 원하는지 간단히 설명

**세부 요구사항**:
- 요구사항 1
- 요구사항 2

**참고**:
추가 정보나 레퍼런스
```

---

## 다음 작업 (TODO)

### Unity 에셋 생성 필요
1. `Assets/Resources/EquipmentDatabase.asset` 생성
2. `Assets/Resources/PlayerDefaultSettings.asset`에 기본 장비 설정
3. 각 무기 타입 데이터 생성 (WeaponTypeData)
4. 각 방어구 타입 데이터 생성 (ArmorTypeData)
5. 테스트용 무기/방어구 데이터 생성

### 우선순위 높음
- [ ] 무기별 스킬셋 분리 구현
- [ ] 무기 교체 UI

### 우선순위 중간
- [ ] 인벤토리 시스템
- [ ] 몬스터 기본 AI

### 우선순위 낮음
- [ ] 퀘스트 시스템
- [ ] 던전 시스템

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|-----------|
| 2024-XX-XX | 장비 시스템 추가 (WeaponData, ArmorData) |
| 2024-XX-XX | CombatStat 제거, 장비 기반 스탯으로 전환 |
| 2024-XX-XX | 플레이어 저장 시스템 추가 (AES 암호화) |
| 2024-XX-XX | 리팩토링 - 미사용 코드 정리 |

---

## 참고 자료

- Mirror 문서: https://mirror-networking.gitbook.io/
- Unity PlayableGraph: https://docs.unity3d.com/Manual/Playables.html
