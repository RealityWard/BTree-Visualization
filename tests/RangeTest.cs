/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace RangeOperationTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture(3, 1000)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public partial class RangeTests(int degree, int numKeys)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  {
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private List<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBufferHistory = [];
    private List<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBufferHistory = [];
    List<int> keys = [];
    private readonly int _NumberOfKeys = numKeys;
    private Task? _Producer;
    private Task? _Consumer;
    private BTree<Person> _Tree;

    private List<int> asdf = [268930, 374341, 894300, 411431, 201589, 271882, 210510, 97656, 859035, 538648, 228237, 973464, 612627, 279252, 80982, 442891, 286182, 818015, 853125, 311579, 668429, 47086, 204447, 738376, 164424, 904439, 838214, 84467, 299232, 505988, 742858, 976321, 63759, 512309, 877908, 861566, 743338, 533353, 427388, 570804, 970201, 521913, 137486, 924460, 19627, 82714, 732134, 768850, 611303, 605507, 890679, 18494, 123613, 359026, 184895, 605505, 982042, 50365, 207895, 154254, 576917, 380639, 67608, 879880];
    private List<int> qwerty = [];

    /// <summary>
    /// NUnit setup for this class. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [SetUp]
    public void Setup()
    {
      _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
      _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
      _OutputBufferHistory = [];
      _InputBufferHistory = [];
      keys = [];
      qwerty.Add(866147);
      qwerty.Add(848803);
      qwerty.Add(770816);
      qwerty.Add(491362);
      qwerty.Add(305452);
      qwerty.Add(387597);
      qwerty.Add(100385);
      qwerty.Add(543226);
      qwerty.Add(190411);
      qwerty.Add(115354);
      qwerty.Add(65572);
      qwerty.Add(29437);
      qwerty.Add(774412);
      qwerty.Add(705139);
      qwerty.Add(548833);
      qwerty.Add(500807);
      qwerty.Add(416752);
      qwerty.Add(201798);
      qwerty.Add(802666);
      qwerty.Add(545843);
      qwerty.Add(545518);
      qwerty.Add(861566);
      qwerty.Add(334077);
      qwerty.Add(791249);
      qwerty.Add(163306);
      qwerty.Add(617205);
      qwerty.Add(311579);
      qwerty.Add(741063);
      qwerty.Add(243420);
      qwerty.Add(758124);
      qwerty.Add(949524);
      qwerty.Add(978088);
      qwerty.Add(607752);
      qwerty.Add(267842);
      qwerty.Add(924460);
      qwerty.Add(219201);
      qwerty.Add(50365);
      qwerty.Add(940787);
      qwerty.Add(538725);
      qwerty.Add(472180);
      qwerty.Add(732134);
      qwerty.Add(730965);
      qwerty.Add(683404);
      qwerty.Add(254798);
      qwerty.Add(727931);
      qwerty.Add(846471);
      qwerty.Add(611591);
      qwerty.Add(632277);
      qwerty.Add(4953);
      qwerty.Add(476595);
      qwerty.Add(663556);
      qwerty.Add(792941);
      qwerty.Add(362445);
      qwerty.Add(785260);
      qwerty.Add(521678);
      qwerty.Add(18258);
      qwerty.Add(404080);
      qwerty.Add(140844);
      qwerty.Add(738376);
      qwerty.Add(355938);
      qwerty.Add(592253);
      qwerty.Add(406666);
      qwerty.Add(781956);
      qwerty.Add(54607);
      qwerty.Add(57023);
      qwerty.Add(53606);
      qwerty.Add(441247);
      qwerty.Add(287765);
      qwerty.Add(330169);
      qwerty.Add(487368);
      qwerty.Add(189241);
      qwerty.Add(657932);
      qwerty.Add(969780);
      qwerty.Add(332299);
      qwerty.Add(542224);
      qwerty.Add(760456);
      qwerty.Add(297949);
      qwerty.Add(377000);
      qwerty.Add(593602);
      qwerty.Add(607920);
      qwerty.Add(348949);
      qwerty.Add(430564);
      qwerty.Add(484871);
      qwerty.Add(811781);
      qwerty.Add(737628);
      qwerty.Add(604520);
      qwerty.Add(372483);
      qwerty.Add(619194);
      qwerty.Add(3900);
      qwerty.Add(972357);
      qwerty.Add(121464);
      qwerty.Add(137486);
      qwerty.Add(475049);
      qwerty.Add(245325);
      qwerty.Add(982042);
      qwerty.Add(988598);
      qwerty.Add(956336);
      qwerty.Add(92213);
      qwerty.Add(855502);
      qwerty.Add(726991);
      qwerty.Add(528922);
      qwerty.Add(906437);
      qwerty.Add(698549);
      qwerty.Add(772609);
      qwerty.Add(4397);
      qwerty.Add(319288);
      qwerty.Add(978251);
      qwerty.Add(972259);
      qwerty.Add(605164);
      qwerty.Add(620196);
      qwerty.Add(142750);
      qwerty.Add(579549);
      qwerty.Add(19007);
      qwerty.Add(439656);
      qwerty.Add(973464);
      qwerty.Add(772347);
      qwerty.Add(811002);
      qwerty.Add(350894);
      qwerty.Add(187349);
      qwerty.Add(841966);
      qwerty.Add(473447);
      qwerty.Add(287917);
      qwerty.Add(632648);
      qwerty.Add(949773);
      qwerty.Add(283451);
      qwerty.Add(573719);
      qwerty.Add(882417);
      qwerty.Add(555391);
      qwerty.Add(914135);
      qwerty.Add(317418);
      qwerty.Add(611303);
      qwerty.Add(718655);
      qwerty.Add(64080);
      qwerty.Add(195781);
      qwerty.Add(136404);
      qwerty.Add(830473);
      qwerty.Add(477987);
      qwerty.Add(683605);
      qwerty.Add(152373);
      qwerty.Add(323891);
      qwerty.Add(827181);
      qwerty.Add(263261);
      qwerty.Add(512309);
      qwerty.Add(440032);
      qwerty.Add(271882);
      qwerty.Add(67608);
      qwerty.Add(221139);
      qwerty.Add(46320);
      qwerty.Add(989478);
      qwerty.Add(762756);
      qwerty.Add(306482);
      qwerty.Add(782571);
      qwerty.Add(270694);
      qwerty.Add(956323);
      qwerty.Add(838214);
      qwerty.Add(782175);
      qwerty.Add(451466);
      qwerty.Add(427108);
      qwerty.Add(31463);
      qwerty.Add(559312);
      qwerty.Add(264310);
      qwerty.Add(114153);
      qwerty.Add(64836);
      qwerty.Add(311556);
      qwerty.Add(956595);
      qwerty.Add(725765);
      qwerty.Add(245512);
      qwerty.Add(773085);
      qwerty.Add(943844);
      qwerty.Add(406652);
      qwerty.Add(187100);
      qwerty.Add(209539);
      qwerty.Add(420733);
      qwerty.Add(949059);
      qwerty.Add(373375);
      qwerty.Add(29926);
      qwerty.Add(627149);
      qwerty.Add(669956);
      qwerty.Add(534575);
      qwerty.Add(452925);
      qwerty.Add(762558);
      qwerty.Add(230131);
      qwerty.Add(89026);
      qwerty.Add(580514);
      qwerty.Add(134419);
      qwerty.Add(533872);
      qwerty.Add(124312);
      qwerty.Add(572714);
      qwerty.Add(437541);
      qwerty.Add(798824);
      qwerty.Add(3181);
      qwerty.Add(843312);
      qwerty.Add(394114);
      qwerty.Add(255977);
      qwerty.Add(256890);
      qwerty.Add(768376);
      qwerty.Add(524047);
      qwerty.Add(741366);
      qwerty.Add(166803);
      qwerty.Add(185020);
      qwerty.Add(200268);
      qwerty.Add(535767);
      qwerty.Add(105649);
      qwerty.Add(746008);
      qwerty.Add(532552);
      qwerty.Add(556891);
      qwerty.Add(700437);
      qwerty.Add(144662);
      qwerty.Add(382672);
      qwerty.Add(745367);
      qwerty.Add(981363);
      qwerty.Add(152680);
      qwerty.Add(826172);
      qwerty.Add(506188);
      qwerty.Add(576917);
      qwerty.Add(933477);
      qwerty.Add(543397);
      qwerty.Add(662326);
      qwerty.Add(629025);
      qwerty.Add(500577);
      qwerty.Add(734284);
      qwerty.Add(682245);
      qwerty.Add(736133);
      qwerty.Add(137302);
      qwerty.Add(901918);
      qwerty.Add(821265);
      qwerty.Add(957722);
      qwerty.Add(501515);
      qwerty.Add(462906);
      qwerty.Add(865741);
      qwerty.Add(805050);
      qwerty.Add(696294);
      qwerty.Add(298648);
      qwerty.Add(854834);
      qwerty.Add(361151);
      qwerty.Add(316384);
      qwerty.Add(780453);
      qwerty.Add(179208);
      qwerty.Add(7457);
      qwerty.Add(830991);
      qwerty.Add(274315);
      qwerty.Add(140909);
      qwerty.Add(587089);
      qwerty.Add(145392);
      qwerty.Add(463529);
      qwerty.Add(629552);
      qwerty.Add(615885);
      qwerty.Add(48077);
      qwerty.Add(721182);
      qwerty.Add(181360);
      qwerty.Add(34284);
      qwerty.Add(605505);
      qwerty.Add(481783);
      qwerty.Add(55442);
      qwerty.Add(84467);
      qwerty.Add(190879);
      qwerty.Add(696909);
      qwerty.Add(16386);
      qwerty.Add(257694);
      qwerty.Add(598827);
      qwerty.Add(897836);
      qwerty.Add(243737);
      qwerty.Add(609996);
      qwerty.Add(53051);
      qwerty.Add(125314);
      qwerty.Add(273027);
      qwerty.Add(469798);
      qwerty.Add(366952);
      qwerty.Add(298596);
      qwerty.Add(931094);
      qwerty.Add(164424);
      qwerty.Add(590824);
      qwerty.Add(43810);
      qwerty.Add(653509);
      qwerty.Add(806409);
      qwerty.Add(833911);
      qwerty.Add(393443);
      qwerty.Add(538648);
      qwerty.Add(122231);
      qwerty.Add(409270);
      qwerty.Add(902183);
      qwerty.Add(521913);
      qwerty.Add(221410);
      qwerty.Add(998232);
      qwerty.Add(452119);
      qwerty.Add(66911);
      qwerty.Add(242790);
      qwerty.Add(111478);
      qwerty.Add(82714);
      qwerty.Add(934246);
      qwerty.Add(400067);
      qwerty.Add(618657);
      qwerty.Add(150417);
      qwerty.Add(741107);
      qwerty.Add(529528);
      qwerty.Add(668429);
      qwerty.Add(222469);
      qwerty.Add(864320);
      qwerty.Add(397451);
      qwerty.Add(704806);
      qwerty.Add(329342);
      qwerty.Add(964036);
      qwerty.Add(714960);
      qwerty.Add(645532);
      qwerty.Add(349390);
      qwerty.Add(485306);
      qwerty.Add(751549);
      qwerty.Add(617288);
      qwerty.Add(338965);
      qwerty.Add(919043);
      qwerty.Add(680083);
      qwerty.Add(149710);
      qwerty.Add(320088);
      qwerty.Add(659087);
      qwerty.Add(564487);
      qwerty.Add(462994);
      qwerty.Add(169103);
      qwerty.Add(996588);
      qwerty.Add(139800);
      qwerty.Add(354749);
      qwerty.Add(669181);
      qwerty.Add(299232);
      qwerty.Add(983792);
      qwerty.Add(201589);
      qwerty.Add(651926);
      qwerty.Add(742858);
      qwerty.Add(645596);
      qwerty.Add(177278);
      qwerty.Add(969988);
      qwerty.Add(678223);
      qwerty.Add(381702);
      qwerty.Add(367887);
      qwerty.Add(750029);
      qwerty.Add(160642);
      qwerty.Add(335269);
      qwerty.Add(75227);
      qwerty.Add(61359);
      qwerty.Add(400954);
      qwerty.Add(981740);
      qwerty.Add(401176);
      qwerty.Add(438498);
      qwerty.Add(223869);
      qwerty.Add(604508);
      qwerty.Add(673504);
      qwerty.Add(508115);
      qwerty.Add(183841);
      qwerty.Add(244229);
      qwerty.Add(634973);
      qwerty.Add(817420);
      qwerty.Add(394752);
      qwerty.Add(974872);
      qwerty.Add(308148);
      qwerty.Add(65279);
      qwerty.Add(196230);
      qwerty.Add(212148);
      qwerty.Add(166399);
      qwerty.Add(995403);
      qwerty.Add(725407);
      qwerty.Add(207895);
      qwerty.Add(699152);
      qwerty.Add(279252);
      qwerty.Add(16413);
      qwerty.Add(991801);
      qwerty.Add(726663);
      qwerty.Add(228237);
      qwerty.Add(569160);
      qwerty.Add(845245);
      qwerty.Add(971321);
      qwerty.Add(381371);
      qwerty.Add(872535);
      qwerty.Add(792057);
      qwerty.Add(565026);
      qwerty.Add(631513);
      qwerty.Add(818626);
      qwerty.Add(861221);
      qwerty.Add(667532);
      qwerty.Add(779095);
      qwerty.Add(551283);
      qwerty.Add(105560);
      qwerty.Add(439955);
      qwerty.Add(647241);
      qwerty.Add(475462);
      qwerty.Add(402129);
      qwerty.Add(471778);
      qwerty.Add(822418);
      qwerty.Add(66347);
      qwerty.Add(166027);
      qwerty.Add(283186);
      qwerty.Add(463871);
      qwerty.Add(698318);
      qwerty.Add(999462);
      qwerty.Add(976321);
      qwerty.Add(262202);
      qwerty.Add(936714);
      qwerty.Add(460969);
      qwerty.Add(84666);
      qwerty.Add(268930);
      qwerty.Add(818015);
      qwerty.Add(967298);
      qwerty.Add(548011);
      qwerty.Add(785728);
      qwerty.Add(65194);
      qwerty.Add(533578);
      qwerty.Add(509656);
      qwerty.Add(944079);
      qwerty.Add(647058);
      qwerty.Add(711959);
      qwerty.Add(661533);
      qwerty.Add(569396);
      qwerty.Add(988979);
      qwerty.Add(275488);
      qwerty.Add(342716);
      qwerty.Add(427250);
      qwerty.Add(471680);
      qwerty.Add(821659);
      qwerty.Add(83327);
      qwerty.Add(819418);
      qwerty.Add(533608);
      qwerty.Add(609457);
      qwerty.Add(32822);
      qwerty.Add(470337);
      qwerty.Add(913515);
      qwerty.Add(278989);
      qwerty.Add(495889);
      qwerty.Add(374341);
      qwerty.Add(135145);
      qwerty.Add(463888);
      qwerty.Add(105519);
      qwerty.Add(80003);
      qwerty.Add(151674);
      qwerty.Add(986540);
      qwerty.Add(594018);
      qwerty.Add(82204);
      qwerty.Add(230928);
      qwerty.Add(699646);
      qwerty.Add(857977);
      qwerty.Add(998824);
      qwerty.Add(724508);
      qwerty.Add(300528);
      qwerty.Add(988775);
      qwerty.Add(286182);
      qwerty.Add(252326);
      qwerty.Add(660657);
      qwerty.Add(66334);
      qwerty.Add(851871);
      qwerty.Add(218401);
      qwerty.Add(359026);
      qwerty.Add(947153);
      qwerty.Add(820242);
      qwerty.Add(667457);
      qwerty.Add(958562);
      qwerty.Add(838484);
      qwerty.Add(428997);
      qwerty.Add(587900);
      qwerty.Add(832964);
      qwerty.Add(830565);
      qwerty.Add(542216);
      qwerty.Add(108201);
      qwerty.Add(7902);
      qwerty.Add(197373);
      qwerty.Add(957022);
      qwerty.Add(479921);
      qwerty.Add(259107);
      qwerty.Add(757902);
      qwerty.Add(29224);
      qwerty.Add(539540);
      qwerty.Add(49332);
      qwerty.Add(812846);
      qwerty.Add(399193);
      qwerty.Add(311456);
      qwerty.Add(725883);
      qwerty.Add(278883);
      qwerty.Add(692804);
      qwerty.Add(80982);
      qwerty.Add(947046);
      qwerty.Add(285996);
      qwerty.Add(149419);
      qwerty.Add(682751);
      qwerty.Add(774633);
      qwerty.Add(570804);
      qwerty.Add(840588);
      qwerty.Add(254706);
      qwerty.Add(715185);
      qwerty.Add(506558);
      qwerty.Add(559145);
      qwerty.Add(576748);
      qwerty.Add(651005);
      qwerty.Add(248377);
      qwerty.Add(434894);
      qwerty.Add(483898);
      qwerty.Add(616980);
      qwerty.Add(936213);
      qwerty.Add(612627);
      qwerty.Add(339321);
      qwerty.Add(865516);
      qwerty.Add(170107);
      qwerty.Add(387559);
      qwerty.Add(856002);
      qwerty.Add(298901);
      qwerty.Add(56741);
      qwerty.Add(499938);
      qwerty.Add(353877);
      qwerty.Add(747679);
      qwerty.Add(801138);
      qwerty.Add(968646);
      qwerty.Add(354979);
      qwerty.Add(785355);
      qwerty.Add(658266);
      qwerty.Add(580485);
      qwerty.Add(186452);
      qwerty.Add(599360);
      qwerty.Add(217379);
      qwerty.Add(609940);
      qwerty.Add(311875);
      qwerty.Add(212962);
      qwerty.Add(530157);
      qwerty.Add(431668);
      qwerty.Add(146537);
      qwerty.Add(114196);
      qwerty.Add(673051);
      qwerty.Add(849547);
      qwerty.Add(896516);
      qwerty.Add(210510);
      qwerty.Add(561320);
      qwerty.Add(550119);
      qwerty.Add(684532);
      qwerty.Add(709914);
      qwerty.Add(453398);
      qwerty.Add(47086);
      qwerty.Add(5952);
      qwerty.Add(184895);
      qwerty.Add(831491);
      qwerty.Add(576956);
      qwerty.Add(97656);
      qwerty.Add(789686);
      qwerty.Add(220413);
      qwerty.Add(267181);
      qwerty.Add(943840);
      qwerty.Add(554953);
      qwerty.Add(890716);
      qwerty.Add(204700);
      qwerty.Add(132546);
      qwerty.Add(303736);
      qwerty.Add(525563);
      qwerty.Add(668873);
      qwerty.Add(142291);
      qwerty.Add(154254);
      qwerty.Add(708082);
      qwerty.Add(535993);
      qwerty.Add(470961);
      qwerty.Add(434571);
      qwerty.Add(47320);
      qwerty.Add(887414);
      qwerty.Add(864884);
      qwerty.Add(618981);
      qwerty.Add(604005);
      qwerty.Add(897926);
      qwerty.Add(853125);
      qwerty.Add(92903);
      qwerty.Add(426951);
      qwerty.Add(252490);
      qwerty.Add(464094);
      qwerty.Add(639039);
      qwerty.Add(748928);
      qwerty.Add(462705);
      qwerty.Add(323536);
      qwerty.Add(540214);
      qwerty.Add(672574);
      qwerty.Add(799828);
      qwerty.Add(263011);
      qwerty.Add(788552);
      qwerty.Add(565462);
      qwerty.Add(260872);
      qwerty.Add(370687);
      qwerty.Add(600516);
      qwerty.Add(846662);
      qwerty.Add(24580);
      qwerty.Add(877908);
      qwerty.Add(550524);
      qwerty.Add(982052);
      qwerty.Add(506921);
      qwerty.Add(142846);
      qwerty.Add(676825);
      qwerty.Add(586197);
      qwerty.Add(667235);
      qwerty.Add(826549);
      qwerty.Add(267505);
      qwerty.Add(773619);
      qwerty.Add(133703);
      qwerty.Add(458231);
      qwerty.Add(281588);
      qwerty.Add(506517);
      qwerty.Add(247936);
      qwerty.Add(18494);
      qwerty.Add(489933);
      qwerty.Add(655311);
      qwerty.Add(549290);
      qwerty.Add(38583);
      qwerty.Add(90324);
      qwerty.Add(664102);
      qwerty.Add(631356);
      qwerty.Add(867556);
      qwerty.Add(790430);
      qwerty.Add(684576);
      qwerty.Add(646856);
      qwerty.Add(240169);
      qwerty.Add(832316);
      qwerty.Add(151850);
      qwerty.Add(66805);
      qwerty.Add(885646);
      qwerty.Add(807271);
      qwerty.Add(396239);
      qwerty.Add(247119);
      qwerty.Add(128049);
      qwerty.Add(852681);
      qwerty.Add(456069);
      qwerty.Add(895612);
      qwerty.Add(60704);
      qwerty.Add(906865);
      qwerty.Add(626648);
      qwerty.Add(859374);
      qwerty.Add(519975);
      qwerty.Add(927495);
      qwerty.Add(728909);
      qwerty.Add(293203);
      qwerty.Add(492697);
      qwerty.Add(543200);
      qwerty.Add(609416);
      qwerty.Add(198168);
      qwerty.Add(452382);
      qwerty.Add(213661);
      qwerty.Add(858763);
      qwerty.Add(708489);
      qwerty.Add(550269);
      qwerty.Add(218998);
      qwerty.Add(165888);
      qwerty.Add(304716);
      qwerty.Add(85107);
      qwerty.Add(967797);
      qwerty.Add(878647);
      qwerty.Add(361839);
      qwerty.Add(562748);
      qwerty.Add(380973);
      qwerty.Add(283472);
      qwerty.Add(765582);
      qwerty.Add(573536);
      qwerty.Add(109135);
      qwerty.Add(395808);
      qwerty.Add(269979);
      qwerty.Add(777472);
      qwerty.Add(304243);
      qwerty.Add(576050);
      qwerty.Add(44823);
      qwerty.Add(524313);
      qwerty.Add(229094);
      qwerty.Add(511489);
      qwerty.Add(485212);
      qwerty.Add(563309);
      qwerty.Add(15790);
      qwerty.Add(637357);
      qwerty.Add(326141);
      qwerty.Add(930927);
      qwerty.Add(839438);
      qwerty.Add(982351);
      qwerty.Add(161092);
      qwerty.Add(581931);
      qwerty.Add(334674);
      qwerty.Add(859035);
      qwerty.Add(839864);
      qwerty.Add(768850);
      qwerty.Add(925837);
      qwerty.Add(280490);
      qwerty.Add(179922);
      qwerty.Add(43623);
      qwerty.Add(474450);
      qwerty.Add(290665);
      qwerty.Add(921926);
      qwerty.Add(122901);
      qwerty.Add(36497);
      qwerty.Add(205450);
      qwerty.Add(876879);
      qwerty.Add(252432);
      qwerty.Add(665974);
      qwerty.Add(518106);
      qwerty.Add(786269);
      qwerty.Add(904439);
      qwerty.Add(805383);
      qwerty.Add(389337);
      qwerty.Add(146352);
      qwerty.Add(909549);
      qwerty.Add(780247);
      qwerty.Add(31287);
      qwerty.Add(278467);
      qwerty.Add(290475);
      qwerty.Add(296864);
      qwerty.Add(165261);
      qwerty.Add(175352);
      qwerty.Add(992003);
      qwerty.Add(761473);
      qwerty.Add(965056);
      qwerty.Add(374535);
      qwerty.Add(79055);
      qwerty.Add(472606);
      qwerty.Add(611507);
      qwerty.Add(860513);
      qwerty.Add(818312);
      qwerty.Add(760370);
      qwerty.Add(418999);
      qwerty.Add(891955);
      qwerty.Add(643946);
      qwerty.Add(657590);
      qwerty.Add(978773);
      qwerty.Add(956675);
      qwerty.Add(995532);
      qwerty.Add(849829);
      qwerty.Add(951993);
      qwerty.Add(431671);
      qwerty.Add(251248);
      qwerty.Add(959914);
      qwerty.Add(157906);
      qwerty.Add(878408);
      qwerty.Add(210335);
      qwerty.Add(505988);
      qwerty.Add(589437);
      qwerty.Add(630503);
      qwerty.Add(269731);
      qwerty.Add(805097);
      qwerty.Add(955497);
      qwerty.Add(927372);
      qwerty.Add(225529);
      qwerty.Add(963954);
      qwerty.Add(16072);
      qwerty.Add(181605);
      qwerty.Add(405374);
      qwerty.Add(386526);
      qwerty.Add(375934);
      qwerty.Add(885490);
      qwerty.Add(125348);
      qwerty.Add(416172);
      qwerty.Add(556082);
      qwerty.Add(799138);
      qwerty.Add(733580);
      qwerty.Add(148107);
      qwerty.Add(14636);
      qwerty.Add(107435);
      qwerty.Add(190456);
      qwerty.Add(68789);
      qwerty.Add(126877);
      qwerty.Add(853455);
      qwerty.Add(554703);
      qwerty.Add(69712);
      qwerty.Add(974185);
      qwerty.Add(157252);
      qwerty.Add(311595);
      qwerty.Add(957448);
      qwerty.Add(894300);
      qwerty.Add(487444);
      qwerty.Add(303457);
      qwerty.Add(202623);
      qwerty.Add(879880);
      qwerty.Add(882210);
      qwerty.Add(17571);
      qwerty.Add(118843);
      qwerty.Add(311435);
      qwerty.Add(117952);
      qwerty.Add(66316);
      qwerty.Add(84807);
      qwerty.Add(142763);
      qwerty.Add(174560);
      qwerty.Add(739118);
      qwerty.Add(253802);
      qwerty.Add(335591);
      qwerty.Add(128596);
      qwerty.Add(17056);
      qwerty.Add(620000);
      qwerty.Add(364228);
      qwerty.Add(881836);
      qwerty.Add(521416);
      qwerty.Add(304907);
      qwerty.Add(661971);
      qwerty.Add(682293);
      qwerty.Add(808510);
      qwerty.Add(871440);
      qwerty.Add(60226);
      qwerty.Add(178866);
      qwerty.Add(940164);
      qwerty.Add(661083);
      qwerty.Add(803864);
      qwerty.Add(882837);
      qwerty.Add(967460);
      qwerty.Add(675727);
      qwerty.Add(257072);
      qwerty.Add(123613);
      qwerty.Add(167751);
      qwerty.Add(258125);
      qwerty.Add(706054);
      qwerty.Add(370891);
      qwerty.Add(877158);
      qwerty.Add(430522);
      qwerty.Add(65552);
      qwerty.Add(307141);
      qwerty.Add(501793);
      qwerty.Add(846175);
      qwerty.Add(180415);
      qwerty.Add(712515);
      qwerty.Add(660801);
      qwerty.Add(121810);
      qwerty.Add(411431);
      qwerty.Add(664089);
      qwerty.Add(268944);
      qwerty.Add(911700);
      qwerty.Add(996453);
      qwerty.Add(32305);
      qwerty.Add(803848);
      qwerty.Add(816243);
      qwerty.Add(771180);
      qwerty.Add(3710);
      qwerty.Add(740736);
      qwerty.Add(953719);
      qwerty.Add(928922);
      qwerty.Add(712495);
      qwerty.Add(442891);
      qwerty.Add(292553);
      qwerty.Add(608666);
      qwerty.Add(653745);
      qwerty.Add(741983);
      qwerty.Add(398919);
      qwerty.Add(670219);
      qwerty.Add(210038);
      qwerty.Add(645717);
      qwerty.Add(412722);
      qwerty.Add(937081);
      qwerty.Add(390127);
      qwerty.Add(436631);
      qwerty.Add(842278);
      qwerty.Add(162595);
      qwerty.Add(298320);
      qwerty.Add(560719);
      qwerty.Add(327134);
      qwerty.Add(486461);
      qwerty.Add(49113);
      qwerty.Add(942827);
      qwerty.Add(726624);
      qwerty.Add(787867);
      qwerty.Add(623887);
      qwerty.Add(461948);
      qwerty.Add(739093);
      qwerty.Add(273077);
      qwerty.Add(457397);
      qwerty.Add(430598);
      qwerty.Add(411054);
      qwerty.Add(561650);
      qwerty.Add(305992);
      qwerty.Add(659328);
      qwerty.Add(350897);
      qwerty.Add(841332);
      qwerty.Add(335404);
      qwerty.Add(412731);
      qwerty.Add(459042);
      qwerty.Add(983723);
      qwerty.Add(204447);
      qwerty.Add(95674);
      qwerty.Add(965518);
      qwerty.Add(931040);
      qwerty.Add(347216);
      qwerty.Add(944608);
      qwerty.Add(65842);
      qwerty.Add(304392);
      qwerty.Add(25078);
      qwerty.Add(968150);
      qwerty.Add(970201);
      qwerty.Add(406573);
      qwerty.Add(534181);
      qwerty.Add(556119);
      qwerty.Add(47462);
      qwerty.Add(825393);
      qwerty.Add(380639);
      qwerty.Add(381912);
      qwerty.Add(533353);
      qwerty.Add(872284);
      qwerty.Add(487061);
      qwerty.Add(207261);
      qwerty.Add(269518);
      qwerty.Add(210856);
      qwerty.Add(414962);
      qwerty.Add(605507);
      qwerty.Add(859281);
      qwerty.Add(919977);
      qwerty.Add(996077);
      qwerty.Add(123103);
      qwerty.Add(933358);
      qwerty.Add(780958);
      qwerty.Add(545787);
      qwerty.Add(894977);
      qwerty.Add(31364);
      qwerty.Add(331188);
      qwerty.Add(350919);
      qwerty.Add(255798);
      qwerty.Add(843298);
      qwerty.Add(620799);
      qwerty.Add(218487);
      qwerty.Add(19627);
      qwerty.Add(864581);
      qwerty.Add(810239);
      qwerty.Add(659782);
      qwerty.Add(331517);
      qwerty.Add(580631);
      qwerty.Add(52115);
      qwerty.Add(229355);
      qwerty.Add(154317);
      qwerty.Add(297242);
      qwerty.Add(709244);
      qwerty.Add(161114);
      qwerty.Add(87833);
      qwerty.Add(580964);
      qwerty.Add(321910);
      qwerty.Add(123412);
      qwerty.Add(653147);
      qwerty.Add(623525);
      qwerty.Add(79815);
      qwerty.Add(327111);
      qwerty.Add(202994);
      qwerty.Add(889535);
      qwerty.Add(15442);
      qwerty.Add(639088);
      qwerty.Add(220894);
      qwerty.Add(821671);
      qwerty.Add(886556);
      qwerty.Add(359832);
      qwerty.Add(173411);
      qwerty.Add(896753);
      qwerty.Add(737857);
      qwerty.Add(335518);
      qwerty.Add(195803);
      qwerty.Add(743338);
      qwerty.Add(317610);
      qwerty.Add(247202);
      qwerty.Add(396959);
      qwerty.Add(250388);
      qwerty.Add(44364);
      qwerty.Add(349959);
      qwerty.Add(339483);
      qwerty.Add(97822);
      qwerty.Add(331116);
      qwerty.Add(470928);
      qwerty.Add(827570);
      qwerty.Add(319239);
      qwerty.Add(914421);
      qwerty.Add(595215);
      qwerty.Add(175549);
      qwerty.Add(642425);
      qwerty.Add(776269);
      qwerty.Add(833441);
      qwerty.Add(409201);
      qwerty.Add(383926);
      qwerty.Add(188468);
      qwerty.Add(107422);
      qwerty.Add(359281);
      qwerty.Add(235078);
      qwerty.Add(355039);
      qwerty.Add(562993);
      qwerty.Add(673278);
      qwerty.Add(814062);
      qwerty.Add(177914);
      qwerty.Add(344405);
      qwerty.Add(543732);
      qwerty.Add(955541);
      qwerty.Add(892090);
      qwerty.Add(78128);
      qwerty.Add(430089);
      qwerty.Add(890679);
      qwerty.Add(427388);
      qwerty.Add(744549);
      qwerty.Add(63759);
      qwerty.Add(143503);
      qwerty.Add(390826);
      qwerty.Add(671532);
      qwerty.Add(410359);
      qwerty.Add(31192);
      qwerty.Add(97568);
      qwerty.Add(430517);
      qwerty.Add(136626);
      qwerty.Add(479936);
      qwerty.Add(212698);
      qwerty.Add(358788);
      qwerty.Add(377311);
      qwerty.Add(855239);
      qwerty.Add(939461);
      qwerty.Add(122510);
      qwerty.Add(832238);
      qwerty.Add(998693);
      qwerty.Add(853261);
      qwerty.Add(831262);
      qwerty.Add(715309);
      qwerty.Add(648020);
      qwerty.Add(170161);
      qwerty.Add(570919);
      qwerty.Add(972863);
      qwerty.Add(613926);
      qwerty.Add(828822);
      _Tree = new(degree, _OutputBuffer);
      _Producer = TreeProduce();
      _Consumer = GuiConsume();
      _ = TreeSetup();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [TearDown]
    public void Outro()
    {
      _Tree.Close();
    }

    /// <summary>
    /// Task creation for the tree object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the tree.</returns>
    private async Task TreeProduce()
    {
      while (await _InputBuffer.OutputAvailableAsync())
      {
        _InputBufferHistory.Add(_InputBuffer.Receive());
        switch (_InputBufferHistory.Last().action)
        {
#pragma warning disable CS8604 // Possible null reference argument.
          case TreeCommand.Tree:
            _Tree = new(_InputBufferHistory.Last().key, _OutputBuffer);
            break;
          case TreeCommand.Insert:
            _Tree.Insert(_InputBufferHistory.Last().key, _InputBufferHistory.Last().content);
            break;
          case TreeCommand.Delete:
            _Tree.Delete(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.DeleteRange:
            _Tree.DeleteRange(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Search:
            _Tree.Search(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.SearchRange:
            _Tree.Search(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Traverse:
            Console.Write(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            _InputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Console.Write("TreeCommand:{0} not recognized", _InputBufferHistory.Last().action);
            break;
        }
        _InputBufferHistory.Clear();
#pragma warning restore CS8604 // Possible null reference argument.
      }
    }

    /// <summary>
    /// Task creation for the fake consumer "GUI" object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the GUI consumer.</returns>
    private async Task GuiConsume()
    {
      while (await _OutputBuffer.OutputAvailableAsync())
      {
        if (_OutputBuffer.Receive().status == NodeStatus.Close)
          _OutputBuffer.Complete();
      }
    }

    /// <summary>
    /// Part of setup to fill the tree object then close its thread.
    /// </summary>
    /// <returns></returns>
    private async Task TreeSetup()
    {
      Random random = new();
      int key = 0;
      //* Alternative Constant setup
      for (int i = 0; i < qwerty.Count; i++)
      {
        key = qwerty[i];
        await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
        keys.Add(key);
      }
      /*/
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 1000);
        } while (keys.Contains(key));
        await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
        keys.Add(key);
      }
      //*/
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
    }

    [TestCase(1)]
    public void BaseTest(int rangeSize)
    {
      /*
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      List<int> history = [];
      string insertionOrder = string.Join(',', keys);
      while (keys.Count > 0)
      {
        index = random.Next(1, keys.Count - 1);
        key = keys[index];
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < keys.Count && keys[endIndex] >= key && keys[endIndex] < endKey; endIndex++)
        {
          range.Add((keys[endIndex], people[endIndex]));
          keys.RemoveAt(endIndex);
          people.RemoveAt(endIndex);
          history.Add(keys[endIndex]);
        }
        // Console.WriteLine(string.Join(',', range));
        // // Assert.That(_Tree.Search(key), Is.Not.Null);
        // Console.WriteLine($"After Count:{_Tree.Search(key, endKey).Count()}\n Range:{key} - {endKey}");
        Assert.That(_Tree.Search(key, endKey), Is.EqualTo(range), string.Join(',', history) + "\n" + insertionOrder);
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty);
        range.Clear();
      }
      */
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      List<int> history = [];
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      //* Alternative Constant setup
      for (int i = 0; i < asdf.Count; i++)
      {
        index = keys.IndexOf(asdf[i]);
        key = asdf[i];
        /*/
      while (keys.Count > 0)
      {
        index = random.Next(1, keys.Count - 1);
        key = keys[index];
        //*/
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < keys.Count && keys[endIndex] >= key && keys[endIndex] < endKey; endIndex++)
        {
          range.Add((keys[endIndex], new(keys[endIndex].ToString())));
          history.Add(keys[endIndex]);
        }
        for (int f = 0; f < range.Count; f++)
        {
          keys.RemoveAll(x => x == range[f].key);
        }
        for (int k = 0; k < keys.Count; k++)
        {
          var entry = _Tree.Search(keys[k]);
          if (entry == null)
            Assert.Fail($"Search turned up bogus\n{string.Join(',', history)}\n{insertionOrder}");
        }
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty);
        range.Clear();
      }
    }

    /// <summary>
    /// Read out just the portion of the keys[] currently in use.
    /// </summary>
    /// <param name="numKeys">Index to stop at.</param>
    /// <param name="keys">Array of ints</param>
    /// <returns>String of the keys seperated by a ','</returns>
    private static string StringifyKeys(int numKeys, int[] keys)
    {
      string result = "";
      for (int i = 0; i < numKeys; i++)
        result += keys[i] + (i + 1 == numKeys ? "" : ",");
      return result;
    }
  }
}