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

    private List<int> asdf = [];
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
      qwerty.Add(29969);
      qwerty.Add(107459);
      qwerty.Add(742121);
      qwerty.Add(248570);
      qwerty.Add(913238);
      qwerty.Add(121770);
      qwerty.Add(219888);
      qwerty.Add(818547);
      qwerty.Add(392404);
      qwerty.Add(879487);
      qwerty.Add(931740);
      qwerty.Add(383256);
      qwerty.Add(270910);
      qwerty.Add(942457);
      qwerty.Add(875572);
      qwerty.Add(589128);
      qwerty.Add(722268);
      qwerty.Add(495212);
      qwerty.Add(476047);
      qwerty.Add(168240);
      qwerty.Add(450748);
      qwerty.Add(826786);
      qwerty.Add(167040);
      qwerty.Add(700185);
      qwerty.Add(251508);
      qwerty.Add(135902);
      qwerty.Add(229929);
      qwerty.Add(338993);
      qwerty.Add(769919);
      qwerty.Add(40404);
      qwerty.Add(504170);
      qwerty.Add(542312);
      qwerty.Add(553544);
      qwerty.Add(56715);
      qwerty.Add(995494);
      qwerty.Add(494309);
      qwerty.Add(525598);
      qwerty.Add(710722);
      qwerty.Add(780760);
      qwerty.Add(695775);
      qwerty.Add(583697);
      qwerty.Add(118196);
      qwerty.Add(845039);
      qwerty.Add(712300);
      qwerty.Add(463987);
      qwerty.Add(580809);
      qwerty.Add(590195);
      qwerty.Add(313905);
      qwerty.Add(142075);
      qwerty.Add(899890);
      qwerty.Add(988800);
      qwerty.Add(493923);
      qwerty.Add(256378);
      qwerty.Add(732837);
      qwerty.Add(786851);
      qwerty.Add(315183);
      qwerty.Add(649284);
      qwerty.Add(896833);
      qwerty.Add(402281);
      qwerty.Add(373750);
      qwerty.Add(631996);
      qwerty.Add(165140);
      qwerty.Add(468991);
      qwerty.Add(884060);
      qwerty.Add(18762);
      qwerty.Add(704657);
      qwerty.Add(249214);
      qwerty.Add(419231);
      qwerty.Add(138265);
      qwerty.Add(599555);
      qwerty.Add(867204);
      qwerty.Add(204486);
      qwerty.Add(16678);
      qwerty.Add(98435);
      qwerty.Add(640262);
      qwerty.Add(682787);
      qwerty.Add(320330);
      qwerty.Add(134710);
      qwerty.Add(983447);
      qwerty.Add(847616);
      qwerty.Add(908810);
      qwerty.Add(248978);
      qwerty.Add(686664);
      qwerty.Add(252274);
      qwerty.Add(593347);
      qwerty.Add(125938);
      qwerty.Add(5577);
      qwerty.Add(577385);
      qwerty.Add(234964);
      qwerty.Add(348899);
      qwerty.Add(647102);
      qwerty.Add(950598);
      qwerty.Add(131405);
      qwerty.Add(225673);
      qwerty.Add(728268);
      qwerty.Add(477781);
      qwerty.Add(209678);
      qwerty.Add(152162);
      qwerty.Add(515435);
      qwerty.Add(999787);
      qwerty.Add(620558);
      qwerty.Add(293745);
      qwerty.Add(328897);
      qwerty.Add(138915);
      qwerty.Add(245656);
      qwerty.Add(563059);
      qwerty.Add(687255);
      qwerty.Add(191867);
      qwerty.Add(194257);
      qwerty.Add(515361);
      qwerty.Add(99213);
      qwerty.Add(665146);
      qwerty.Add(568128);
      qwerty.Add(713582);
      qwerty.Add(726529);
      qwerty.Add(869945);
      qwerty.Add(359536);
      qwerty.Add(894654);
      qwerty.Add(540800);
      qwerty.Add(541026);
      qwerty.Add(359111);
      qwerty.Add(483489);
      qwerty.Add(294412);
      qwerty.Add(756745);
      qwerty.Add(189818);
      qwerty.Add(261484);
      qwerty.Add(757383);
      qwerty.Add(891255);
      qwerty.Add(52569);
      qwerty.Add(793697);
      qwerty.Add(174716);
      qwerty.Add(401301);
      qwerty.Add(330799);
      qwerty.Add(183405);
      qwerty.Add(87158);
      qwerty.Add(918455);
      qwerty.Add(115607);
      qwerty.Add(291280);
      qwerty.Add(711234);
      qwerty.Add(207439);
      qwerty.Add(559412);
      qwerty.Add(325412);
      qwerty.Add(392712);
      qwerty.Add(853955);
      qwerty.Add(785697);
      qwerty.Add(507593);
      qwerty.Add(211085);
      qwerty.Add(452153);
      qwerty.Add(30173);
      qwerty.Add(670229);
      qwerty.Add(288112);
      qwerty.Add(942143);
      qwerty.Add(370203);
      qwerty.Add(878823);
      qwerty.Add(841100);
      qwerty.Add(822725);
      qwerty.Add(354574);
      qwerty.Add(134406);
      qwerty.Add(379967);
      qwerty.Add(787443);
      qwerty.Add(322680);
      qwerty.Add(422873);
      qwerty.Add(409106);
      qwerty.Add(471874);
      qwerty.Add(412208);
      qwerty.Add(679188);
      qwerty.Add(85744);
      qwerty.Add(936095);
      qwerty.Add(792324);
      qwerty.Add(961008);
      qwerty.Add(962592);
      qwerty.Add(1591);
      qwerty.Add(699360);
      qwerty.Add(600910);
      qwerty.Add(837644);
      qwerty.Add(695281);
      qwerty.Add(894511);
      qwerty.Add(322805);
      qwerty.Add(953614);
      qwerty.Add(833931);
      qwerty.Add(92921);
      qwerty.Add(670412);
      qwerty.Add(480608);
      qwerty.Add(621048);
      qwerty.Add(184785);
      qwerty.Add(748885);
      qwerty.Add(308660);
      qwerty.Add(36973);
      qwerty.Add(429360);
      qwerty.Add(406739);
      qwerty.Add(390358);
      qwerty.Add(633122);
      qwerty.Add(708169);
      qwerty.Add(173495);
      qwerty.Add(849636);
      qwerty.Add(420848);
      qwerty.Add(886703);
      qwerty.Add(498238);
      qwerty.Add(146968);
      qwerty.Add(488297);
      qwerty.Add(80851);
      qwerty.Add(223163);
      qwerty.Add(903640);
      qwerty.Add(318563);
      qwerty.Add(204574);
      qwerty.Add(179870);
      qwerty.Add(215278);
      qwerty.Add(906337);
      qwerty.Add(642614);
      qwerty.Add(399848);
      qwerty.Add(60498);
      qwerty.Add(408868);
      qwerty.Add(615761);
      qwerty.Add(912422);
      qwerty.Add(844057);
      qwerty.Add(546309);
      qwerty.Add(642186);
      qwerty.Add(670747);
      qwerty.Add(690785);
      qwerty.Add(147222);
      qwerty.Add(295292);
      qwerty.Add(772039);
      qwerty.Add(301371);
      qwerty.Add(344722);
      qwerty.Add(221966);
      qwerty.Add(638751);
      qwerty.Add(983912);
      qwerty.Add(329534);
      qwerty.Add(722077);
      qwerty.Add(104328);
      qwerty.Add(875194);
      qwerty.Add(318838);
      qwerty.Add(704741);
      qwerty.Add(201739);
      qwerty.Add(254958);
      qwerty.Add(851042);
      qwerty.Add(428796);
      qwerty.Add(56791);
      qwerty.Add(47787);
      qwerty.Add(653300);
      qwerty.Add(600290);
      qwerty.Add(542904);
      qwerty.Add(990761);
      qwerty.Add(205937);
      qwerty.Add(356539);
      qwerty.Add(242184);
      qwerty.Add(39463);
      qwerty.Add(101915);
      qwerty.Add(43516);
      qwerty.Add(468818);
      qwerty.Add(118065);
      qwerty.Add(27463);
      qwerty.Add(378701);
      qwerty.Add(854650);
      qwerty.Add(637224);
      qwerty.Add(488552);
      qwerty.Add(539275);
      qwerty.Add(31062);
      qwerty.Add(231115);
      qwerty.Add(701564);
      qwerty.Add(171189);
      qwerty.Add(15710);
      qwerty.Add(611401);
      qwerty.Add(389262);
      qwerty.Add(444635);
      qwerty.Add(678806);
      qwerty.Add(549093);
      qwerty.Add(189211);
      qwerty.Add(472516);
      qwerty.Add(631275);
      qwerty.Add(249711);
      qwerty.Add(949037);
      qwerty.Add(874511);
      qwerty.Add(947992);
      qwerty.Add(135588);
      qwerty.Add(481187);
      qwerty.Add(10917);
      qwerty.Add(41959);
      qwerty.Add(388792);
      qwerty.Add(601294);
      qwerty.Add(122325);
      qwerty.Add(30644);
      qwerty.Add(545163);
      qwerty.Add(51908);
      qwerty.Add(839476);
      qwerty.Add(675534);
      qwerty.Add(21766);
      qwerty.Add(422766);
      qwerty.Add(192789);
      qwerty.Add(629775);
      qwerty.Add(158181);
      qwerty.Add(787476);
      qwerty.Add(499875);
      qwerty.Add(199276);
      qwerty.Add(411226);
      qwerty.Add(280321);
      qwerty.Add(51665);
      qwerty.Add(505240);
      qwerty.Add(67345);
      qwerty.Add(352019);
      qwerty.Add(542509);
      qwerty.Add(163328);
      qwerty.Add(408396);
      qwerty.Add(201548);
      qwerty.Add(635256);
      qwerty.Add(923336);
      qwerty.Add(763445);
      qwerty.Add(809595);
      qwerty.Add(357703);
      qwerty.Add(729532);
      qwerty.Add(897041);
      qwerty.Add(165860);
      qwerty.Add(183426);
      qwerty.Add(289293);
      qwerty.Add(523968);
      qwerty.Add(452146);
      qwerty.Add(727740);
      qwerty.Add(401368);
      qwerty.Add(254827);
      qwerty.Add(653086);
      qwerty.Add(249636);
      qwerty.Add(840891);
      qwerty.Add(376499);
      qwerty.Add(285817);
      qwerty.Add(833051);
      qwerty.Add(828809);
      qwerty.Add(282953);
      qwerty.Add(7403);
      qwerty.Add(645674);
      qwerty.Add(275315);
      qwerty.Add(642470);
      qwerty.Add(552208);
      qwerty.Add(412929);
      qwerty.Add(18293);
      qwerty.Add(767862);
      qwerty.Add(922183);
      qwerty.Add(984150);
      qwerty.Add(217106);
      qwerty.Add(272060);
      qwerty.Add(462788);
      qwerty.Add(881966);
      qwerty.Add(563899);
      qwerty.Add(48339);
      qwerty.Add(937526);
      qwerty.Add(793466);
      qwerty.Add(85676);
      qwerty.Add(412101);
      qwerty.Add(576523);
      qwerty.Add(190829);
      qwerty.Add(714906);
      qwerty.Add(189281);
      qwerty.Add(910189);
      qwerty.Add(1441);
      qwerty.Add(155128);
      qwerty.Add(448403);
      qwerty.Add(381478);
      qwerty.Add(868140);
      qwerty.Add(855903);
      qwerty.Add(104144);
      qwerty.Add(634747);
      qwerty.Add(733365);
      qwerty.Add(973104);
      qwerty.Add(748824);
      qwerty.Add(961855);
      qwerty.Add(966998);
      qwerty.Add(874277);
      qwerty.Add(105933);
      qwerty.Add(665562);
      qwerty.Add(599176);
      qwerty.Add(649015);
      qwerty.Add(148596);
      qwerty.Add(390328);
      qwerty.Add(858961);
      qwerty.Add(886158);
      qwerty.Add(7247);
      qwerty.Add(134631);
      qwerty.Add(223608);
      qwerty.Add(828239);
      qwerty.Add(458196);
      qwerty.Add(572132);
      qwerty.Add(906);
      qwerty.Add(289246);
      qwerty.Add(961200);
      qwerty.Add(98526);
      qwerty.Add(306863);
      qwerty.Add(553236);
      qwerty.Add(891001);
      qwerty.Add(655509);
      qwerty.Add(290675);
      qwerty.Add(671599);
      qwerty.Add(330936);
      qwerty.Add(147260);
      qwerty.Add(425504);
      qwerty.Add(375888);
      qwerty.Add(422660);
      qwerty.Add(11543);
      qwerty.Add(249028);
      qwerty.Add(372359);
      qwerty.Add(545595);
      qwerty.Add(735341);
      qwerty.Add(748725);
      qwerty.Add(660808);
      qwerty.Add(982839);
      qwerty.Add(158950);
      qwerty.Add(321350);
      qwerty.Add(818851);
      qwerty.Add(784103);
      qwerty.Add(99454);
      qwerty.Add(604889);
      qwerty.Add(887421);
      qwerty.Add(427870);
      qwerty.Add(520283);
      qwerty.Add(768535);
      qwerty.Add(54945);
      qwerty.Add(355456);
      qwerty.Add(784231);
      qwerty.Add(590875);
      qwerty.Add(76159);
      qwerty.Add(505387);
      qwerty.Add(309311);
      qwerty.Add(89667);
      qwerty.Add(941952);
      qwerty.Add(147373);
      qwerty.Add(446804);
      qwerty.Add(178559);
      qwerty.Add(278373);
      qwerty.Add(800275);
      qwerty.Add(932764);
      qwerty.Add(912188);
      qwerty.Add(555924);
      qwerty.Add(153516);
      qwerty.Add(88041);
      qwerty.Add(545219);
      qwerty.Add(233930);
      qwerty.Add(966312);
      qwerty.Add(351860);
      qwerty.Add(264876);
      qwerty.Add(216623);
      qwerty.Add(223306);
      qwerty.Add(274113);
      qwerty.Add(856522);
      qwerty.Add(968728);
      qwerty.Add(263350);
      qwerty.Add(906624);
      qwerty.Add(808865);
      qwerty.Add(488802);
      qwerty.Add(142055);
      qwerty.Add(172891);
      qwerty.Add(589561);
      qwerty.Add(697829);
      qwerty.Add(920600);
      qwerty.Add(114375);
      qwerty.Add(267107);
      qwerty.Add(72681);
      qwerty.Add(404915);
      qwerty.Add(550512);
      qwerty.Add(429862);
      qwerty.Add(930407);
      qwerty.Add(338314);
      qwerty.Add(694186);
      qwerty.Add(337580);
      qwerty.Add(503177);
      qwerty.Add(721545);
      qwerty.Add(993151);
      qwerty.Add(548155);
      qwerty.Add(644922);
      qwerty.Add(616037);
      qwerty.Add(567405);
      qwerty.Add(74406);
      qwerty.Add(963247);
      qwerty.Add(64005);
      qwerty.Add(847200);
      qwerty.Add(850949);
      qwerty.Add(849872);
      qwerty.Add(218264);
      qwerty.Add(962324);
      qwerty.Add(529652);
      qwerty.Add(749572);
      qwerty.Add(746482);
      qwerty.Add(216014);
      qwerty.Add(605164);
      qwerty.Add(797808);
      qwerty.Add(653741);
      qwerty.Add(742023);
      qwerty.Add(963613);
      qwerty.Add(596938);
      qwerty.Add(291860);
      qwerty.Add(645156);
      qwerty.Add(385753);
      qwerty.Add(251253);
      qwerty.Add(69986);
      qwerty.Add(532381);
      qwerty.Add(37204);
      qwerty.Add(425553);
      qwerty.Add(853513);
      qwerty.Add(636934);
      qwerty.Add(519726);
      qwerty.Add(865107);
      qwerty.Add(333106);
      qwerty.Add(747157);
      qwerty.Add(481082);
      qwerty.Add(882716);
      qwerty.Add(871588);
      qwerty.Add(693044);
      qwerty.Add(408390);
      qwerty.Add(230174);
      qwerty.Add(517090);
      qwerty.Add(830009);
      qwerty.Add(780572);
      qwerty.Add(350390);
      qwerty.Add(748805);
      qwerty.Add(93218);
      qwerty.Add(147256);
      qwerty.Add(393557);
      qwerty.Add(998663);
      qwerty.Add(49788);
      qwerty.Add(802675);
      qwerty.Add(73085);
      qwerty.Add(758794);
      qwerty.Add(397440);
      qwerty.Add(879994);
      qwerty.Add(290068);
      qwerty.Add(622703);
      qwerty.Add(498042);
      qwerty.Add(68773);
      qwerty.Add(447562);
      qwerty.Add(312780);
      qwerty.Add(8643);
      qwerty.Add(864067);
      qwerty.Add(60224);
      qwerty.Add(483234);
      qwerty.Add(863133);
      qwerty.Add(873841);
      qwerty.Add(384882);
      qwerty.Add(294681);
      qwerty.Add(901641);
      qwerty.Add(546594);
      qwerty.Add(451454);
      qwerty.Add(769193);
      qwerty.Add(489122);
      qwerty.Add(937136);
      qwerty.Add(848314);
      qwerty.Add(38348);
      qwerty.Add(698178);
      qwerty.Add(695667);
      qwerty.Add(279974);
      qwerty.Add(739703);
      qwerty.Add(137230);
      qwerty.Add(872691);
      qwerty.Add(690427);
      qwerty.Add(653902);
      qwerty.Add(656523);
      qwerty.Add(170995);
      qwerty.Add(875374);
      qwerty.Add(345851);
      qwerty.Add(128216);
      qwerty.Add(527947);
      qwerty.Add(613531);
      qwerty.Add(902948);
      qwerty.Add(215768);
      qwerty.Add(631832);
      qwerty.Add(150941);
      qwerty.Add(440488);
      qwerty.Add(987167);
      qwerty.Add(624965);
      qwerty.Add(524885);
      qwerty.Add(671512);
      qwerty.Add(813655);
      qwerty.Add(738169);
      qwerty.Add(112631);
      qwerty.Add(573393);
      qwerty.Add(700876);
      qwerty.Add(697441);
      qwerty.Add(871491);
      qwerty.Add(250987);
      qwerty.Add(545492);
      qwerty.Add(467379);
      qwerty.Add(308934);
      qwerty.Add(881609);
      qwerty.Add(277436);
      qwerty.Add(799203);
      qwerty.Add(130137);
      qwerty.Add(318622);
      qwerty.Add(229001);
      qwerty.Add(860968);
      qwerty.Add(803886);
      qwerty.Add(853965);
      qwerty.Add(490455);
      qwerty.Add(242443);
      qwerty.Add(335813);
      qwerty.Add(411951);
      qwerty.Add(800987);
      qwerty.Add(809416);
      qwerty.Add(623134);
      qwerty.Add(586320);
      qwerty.Add(767504);
      qwerty.Add(99344);
      qwerty.Add(635749);
      qwerty.Add(348730);
      qwerty.Add(640953);
      qwerty.Add(51038);
      qwerty.Add(374424);
      qwerty.Add(82678);
      qwerty.Add(944476);
      qwerty.Add(80449);
      qwerty.Add(735397);
      qwerty.Add(436437);
      qwerty.Add(558258);
      qwerty.Add(788122);
      qwerty.Add(841247);
      qwerty.Add(667231);
      qwerty.Add(224650);
      qwerty.Add(407095);
      qwerty.Add(293604);
      qwerty.Add(218724);
      qwerty.Add(196455);
      qwerty.Add(565084);
      qwerty.Add(12674);
      qwerty.Add(718557);
      qwerty.Add(192540);
      qwerty.Add(592864);
      qwerty.Add(965795);
      qwerty.Add(250968);
      qwerty.Add(826570);
      qwerty.Add(672257);
      qwerty.Add(704453);
      qwerty.Add(587887);
      qwerty.Add(827752);
      qwerty.Add(468553);
      qwerty.Add(352667);
      qwerty.Add(931366);
      qwerty.Add(781270);
      qwerty.Add(265061);
      qwerty.Add(666301);
      qwerty.Add(663975);
      qwerty.Add(17700);
      qwerty.Add(789437);
      qwerty.Add(927079);
      qwerty.Add(643890);
      qwerty.Add(876140);
      qwerty.Add(421734);
      qwerty.Add(398962);
      qwerty.Add(391815);
      qwerty.Add(737003);
      qwerty.Add(16768);
      qwerty.Add(42679);
      qwerty.Add(780107);
      qwerty.Add(759811);
      qwerty.Add(133269);
      qwerty.Add(428722);
      qwerty.Add(83819);
      qwerty.Add(461288);
      qwerty.Add(274795);
      qwerty.Add(232842);
      qwerty.Add(406067);
      qwerty.Add(231587);
      qwerty.Add(749258);
      qwerty.Add(656018);
      qwerty.Add(696637);
      qwerty.Add(234685);
      qwerty.Add(797242);
      qwerty.Add(197321);
      qwerty.Add(181776);
      qwerty.Add(34971);
      qwerty.Add(644902);
      qwerty.Add(852353);
      qwerty.Add(510334);
      qwerty.Add(413121);
      qwerty.Add(610014);
      qwerty.Add(674409);
      qwerty.Add(323629);
      qwerty.Add(471264);
      qwerty.Add(960117);
      qwerty.Add(551771);
      qwerty.Add(790684);
      qwerty.Add(458580);
      qwerty.Add(386516);
      qwerty.Add(814479);
      qwerty.Add(599072);
      qwerty.Add(214045);
      qwerty.Add(342818);
      qwerty.Add(650991);
      qwerty.Add(737386);
      qwerty.Add(105790);
      qwerty.Add(756023);
      qwerty.Add(279214);
      qwerty.Add(125049);
      qwerty.Add(619588);
      qwerty.Add(816366);
      qwerty.Add(827306);
      qwerty.Add(985133);
      qwerty.Add(847204);
      qwerty.Add(185403);
      qwerty.Add(975787);
      qwerty.Add(968751);
      qwerty.Add(357218);
      qwerty.Add(314640);
      qwerty.Add(292856);
      qwerty.Add(683927);
      qwerty.Add(82096);
      qwerty.Add(219463);
      qwerty.Add(791559);
      qwerty.Add(798082);
      qwerty.Add(597911);
      qwerty.Add(707309);
      qwerty.Add(40004);
      qwerty.Add(441504);
      qwerty.Add(983231);
      qwerty.Add(402072);
      qwerty.Add(937004);
      qwerty.Add(829910);
      qwerty.Add(541333);
      qwerty.Add(818662);
      qwerty.Add(774841);
      qwerty.Add(259455);
      qwerty.Add(964923);
      qwerty.Add(538690);
      qwerty.Add(20649);
      qwerty.Add(782551);
      qwerty.Add(359032);
      qwerty.Add(371449);
      qwerty.Add(695727);
      qwerty.Add(928622);
      qwerty.Add(473389);
      qwerty.Add(297263);
      qwerty.Add(179545);
      qwerty.Add(335391);
      qwerty.Add(172769);
      qwerty.Add(772096);
      qwerty.Add(329015);
      qwerty.Add(765896);
      qwerty.Add(683507);
      qwerty.Add(10029);
      qwerty.Add(134727);
      qwerty.Add(96402);
      qwerty.Add(110585);
      qwerty.Add(996410);
      qwerty.Add(163051);
      qwerty.Add(463472);
      qwerty.Add(445485);
      qwerty.Add(892441);
      qwerty.Add(644141);
      qwerty.Add(858612);
      qwerty.Add(686998);
      qwerty.Add(306821);
      qwerty.Add(906341);
      qwerty.Add(289854);
      qwerty.Add(993056);
      qwerty.Add(611257);
      qwerty.Add(99650);
      qwerty.Add(133483);
      qwerty.Add(746657);
      qwerty.Add(729619);
      qwerty.Add(33497);
      qwerty.Add(352912);
      qwerty.Add(217287);
      qwerty.Add(454497);
      qwerty.Add(676211);
      qwerty.Add(838012);
      qwerty.Add(157574);
      qwerty.Add(6044);
      qwerty.Add(895188);
      qwerty.Add(381321);
      qwerty.Add(325741);
      qwerty.Add(696541);
      qwerty.Add(25082);
      qwerty.Add(772869);
      qwerty.Add(817162);
      qwerty.Add(559814);
      qwerty.Add(637067);
      qwerty.Add(574204);
      qwerty.Add(996773);
      qwerty.Add(193316);
      qwerty.Add(122707);
      qwerty.Add(956640);
      qwerty.Add(266837);
      qwerty.Add(715952);
      qwerty.Add(237772);
      qwerty.Add(937250);
      qwerty.Add(87966);
      qwerty.Add(16379);
      qwerty.Add(331975);
      qwerty.Add(25412);
      qwerty.Add(823393);
      qwerty.Add(884319);
      qwerty.Add(475516);
      qwerty.Add(258156);
      qwerty.Add(243877);
      qwerty.Add(844262);
      qwerty.Add(904839);
      qwerty.Add(432164);
      qwerty.Add(546836);
      qwerty.Add(719668);
      qwerty.Add(704869);
      qwerty.Add(428665);
      qwerty.Add(383322);
      qwerty.Add(838360);
      qwerty.Add(87470);
      qwerty.Add(656969);
      qwerty.Add(596180);
      qwerty.Add(17046);
      qwerty.Add(55352);
      qwerty.Add(690192);
      qwerty.Add(36182);
      qwerty.Add(564077);
      qwerty.Add(523977);
      qwerty.Add(190356);
      qwerty.Add(425289);
      qwerty.Add(933742);
      qwerty.Add(468102);
      qwerty.Add(915186);
      qwerty.Add(31372);
      qwerty.Add(273764);
      qwerty.Add(372141);
      qwerty.Add(955028);
      qwerty.Add(781402);
      qwerty.Add(579262);
      qwerty.Add(620361);
      qwerty.Add(633992);
      qwerty.Add(576487);
      qwerty.Add(662740);
      qwerty.Add(675446);
      qwerty.Add(719078);
      qwerty.Add(39640);
      qwerty.Add(52642);
      qwerty.Add(583157);
      qwerty.Add(941119);
      qwerty.Add(538724);
      qwerty.Add(531129);
      qwerty.Add(955799);
      qwerty.Add(324597);
      qwerty.Add(169211);
      qwerty.Add(367714);
      qwerty.Add(212711);
      qwerty.Add(60085);
      qwerty.Add(227436);
      qwerty.Add(567511);
      qwerty.Add(423747);
      qwerty.Add(160110);
      qwerty.Add(918387);
      qwerty.Add(634662);
      qwerty.Add(428373);
      qwerty.Add(559792);
      qwerty.Add(411477);
      qwerty.Add(693759);
      qwerty.Add(566039);
      qwerty.Add(867482);
      qwerty.Add(492748);
      qwerty.Add(149990);
      qwerty.Add(92955);
      qwerty.Add(923595);
      qwerty.Add(423364);
      qwerty.Add(564116);
      qwerty.Add(301293);
      qwerty.Add(736392);
      qwerty.Add(873213);
      qwerty.Add(621650);
      qwerty.Add(740081);
      qwerty.Add(393372);
      qwerty.Add(389015);
      qwerty.Add(10258);
      qwerty.Add(921579);
      qwerty.Add(47801);
      qwerty.Add(215185);
      qwerty.Add(183633);
      qwerty.Add(375725);
      qwerty.Add(690260);
      qwerty.Add(817566);
      qwerty.Add(244349);
      qwerty.Add(293535);
      qwerty.Add(414111);
      qwerty.Add(816515);
      qwerty.Add(8213);
      qwerty.Add(958052);
      qwerty.Add(92100);
      qwerty.Add(144532);
      qwerty.Add(269576);
      qwerty.Add(731936);
      qwerty.Add(712481);
      qwerty.Add(820609);
      qwerty.Add(580305);
      qwerty.Add(736212);
      qwerty.Add(12724);
      qwerty.Add(161602);
      qwerty.Add(166918);
      qwerty.Add(825145);
      qwerty.Add(64450);
      qwerty.Add(120803);
      qwerty.Add(569140);
      qwerty.Add(201323);
      qwerty.Add(416824);
      qwerty.Add(377308);
      qwerty.Add(729063);
      qwerty.Add(377057);
      qwerty.Add(400817);
      qwerty.Add(626496);
      qwerty.Add(630967);
      qwerty.Add(155373);
      qwerty.Add(465162);
      qwerty.Add(780787);
      qwerty.Add(25720);
      qwerty.Add(451148);
      qwerty.Add(228537);
      qwerty.Add(353655);
      qwerty.Add(748061);
      qwerty.Add(978275);
      qwerty.Add(290629);
      qwerty.Add(446190);
      qwerty.Add(31037);
      qwerty.Add(74957);
      qwerty.Add(983110);
      qwerty.Add(721084);
      qwerty.Add(442033);
      qwerty.Add(786399);
      qwerty.Add(997828);
      qwerty.Add(939687);
      qwerty.Add(255810);
      qwerty.Add(884114);
      qwerty.Add(421555);
      qwerty.Add(490172);
      qwerty.Add(382467);
      qwerty.Add(361602);
      qwerty.Add(480429);
      qwerty.Add(695177);
      qwerty.Add(299313);
      qwerty.Add(967763);
      qwerty.Add(36610);
      qwerty.Add(111560);
      qwerty.Add(256225);
      qwerty.Add(977474);
      qwerty.Add(100583);
      qwerty.Add(746720);
      qwerty.Add(782735);
      qwerty.Add(764043);
      qwerty.Add(985065);
      qwerty.Add(554296);
      qwerty.Add(180182);
      qwerty.Add(35381);
      qwerty.Add(313912);
      qwerty.Add(745679);
      qwerty.Add(689477);
      qwerty.Add(555994);
      qwerty.Add(436737);
      qwerty.Add(930124);
      qwerty.Add(720430);
      qwerty.Add(928552);
      qwerty.Add(481223);
      qwerty.Add(840057);
      qwerty.Add(89048);
      qwerty.Add(7019);
      qwerty.Add(253794);
      qwerty.Add(119033);
      qwerty.Add(219854);
      qwerty.Add(816583);
      qwerty.Add(717658);
      qwerty.Add(539256);
      qwerty.Add(477894);
      qwerty.Add(484753);
      qwerty.Add(911678);
      qwerty.Add(564653);
      qwerty.Add(66733);
      qwerty.Add(225133);
      qwerty.Add(806374);
      qwerty.Add(338353);
      qwerty.Add(188250);
      qwerty.Add(105435);
      qwerty.Add(977037);
      qwerty.Add(958088);
      qwerty.Add(862843);
      qwerty.Add(65283);
      qwerty.Add(90432);
      qwerty.Add(22198);
      qwerty.Add(974343);
      qwerty.Add(247770);
      qwerty.Add(352610);
      qwerty.Add(99951);
      qwerty.Add(285715);
      qwerty.Add(501947);
      qwerty.Add(118289);
      qwerty.Add(311397);
      qwerty.Add(309701);
      qwerty.Add(44308);
      qwerty.Add(826510);
      qwerty.Add(48731);
      qwerty.Add(379563);
      qwerty.Add(323279);
      qwerty.Add(395170);
      qwerty.Add(240411);
      qwerty.Add(411912);
      qwerty.Add(512316);
      qwerty.Add(931798);
      qwerty.Add(863832);
      qwerty.Add(464212);
      qwerty.Add(515490);
      qwerty.Add(787715);
      qwerty.Add(518804);
      qwerty.Add(738246);
      qwerty.Add(476133);
      qwerty.Add(64898);
      qwerty.Add(51713);
      asdf.Add(93218);
      asdf.Add(96402);
      asdf.Add(98435);
      asdf.Add(98526);
      asdf.Add(99213);
      asdf.Add(99344);
      asdf.Add(99454);
      asdf.Add(99650);
      asdf.Add(99951);
      asdf.Add(100583);
      asdf.Add(101915);
      asdf.Add(590875);
      asdf.Add(592864);
      asdf.Add(593347);
      asdf.Add(596180);
      asdf.Add(596938);
      asdf.Add(597911);
      asdf.Add(599072);
      asdf.Add(599176);
      asdf.Add(599555);
      asdf.Add(600290);
      asdf.Add(338993);
      asdf.Add(342818);
      asdf.Add(344722);
      asdf.Add(345851);
      asdf.Add(348730);
      asdf.Add(348899);
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

    // [TestCase(1000)]
    [TestCase(10000)]
    public void BaseTest(int rangeSize)
    {
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      List<int> history = [], keyHistory = [];
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      //* Alternative Constant setup
      while (asdf.Count > 0)
      {
        index = keys.IndexOf(asdf[0]);
        key = asdf[0];
        /*/
      while (keys.Count > 0)
      {
        if (keys.Count - 1 == 0)
          index = 0;
        else
          index = random.Next(1, keys.Count - 1);
        key = keys[index];
        //*/
        keyHistory.Add(key);
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < keys.Count && keys[endIndex] >= key && keys[endIndex] < endKey; endIndex++)
        {
          range.Add((keys[endIndex], new(keys[endIndex].ToString())));
          history.Add(keys[endIndex]);
        }
        for (int f = 0; f < range.Count; f++)
        {
          keys.RemoveAll(x => x == range[f].key);
          asdf.RemoveAll(x => x == range[f].key);
        }
        string deleteHstory = string.Join(',', history);
        for (int k = 0; k < keys.Count; k++)
        {
          var entry = _Tree.Search(keys[k]);
          if (entry == null)
            Assert.Fail($"The delete cycles started with {string.Join(',', keyHistory)}\nSearch turned up bogus for {keys[k]}\n{deleteHstory}\n\n{insertionOrder}");
        }
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty, $"The delete cycles started with {string.Join(',', keyHistory)}\nSearch shouldnt exist for {key}\n{deleteHstory}\n\n{insertionOrder}");
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